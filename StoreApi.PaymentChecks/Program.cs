using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StoreApi.Payments;

var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
    ["PayPal:ClientId"] = "test-id", ["PayPal:ClientSecret"] = "test-secret",
    ["PayPal:WebhookId"] = "test-webhook", ["Supabase:Url"] = "https://example.supabase.co",
    ["Supabase:PublishableKey"] = "test-public"
}).Build();
var calls = new List<(string Path, string? RequestId, string? Body)>();
var handler = new StubHandler(async request => {
    var body = request.Content is null ? null : await request.Content.ReadAsStringAsync();
    calls.Add((request.RequestUri!.AbsolutePath, request.Headers.TryGetValues("PayPal-Request-Id", out var ids) ? ids.Single() : null, body));
    return request.RequestUri.AbsolutePath switch {
        "/v1/oauth2/token" => Json("{" + "\"access_token\":\"test-token\"}"),
        "/v1/notifications/verify-webhook-signature" => Json("{" + "\"verification_status\":\"SUCCESS\"}"),
        "/v2/checkout/orders" => Json("{" + "\"id\":\"ORDER1\"}"),
        "/auth/v1/user" when request.Headers.Authorization?.Parameter == "valid-token-with-length" => Json("{" + "\"id\":\"11111111-1111-1111-1111-111111111111\"}"),
        _ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
    };
});
using var http = new HttpClient(handler);
var paypal = new PayPalClient(http, config);
using var order = await paypal.SendAsync(HttpMethod.Post, "/v2/checkout/orders", new { intent = "CAPTURE" }, "c-11111111111111111111111111111111", default);
Check(order.RootElement.GetProperty("id").GetString() == "ORDER1", "PayPal response");
Check(calls.Any(x => x.Path == "/v2/checkout/orders" && x.RequestId == "c-11111111111111111111111111111111"), "stable idempotency header");
Check(calls.All(x => x.Body?.Contains("test-secret", StringComparison.Ordinal) != true), "secret absent from JSON bodies");
var headers = new HeaderDictionary {
    ["PAYPAL-AUTH-ALGO"] = "SHA256withRSA", ["PAYPAL-CERT-URL"] = "https://example.com/cert",
    ["PAYPAL-TRANSMISSION-ID"] = "T1", ["PAYPAL-TRANSMISSION-SIG"] = "signature",
    ["PAYPAL-TRANSMISSION-TIME"] = "2026-09-17T00:00:00Z"
};
using var eventJson = JsonDocument.Parse("{\"id\":\"WH1\"}");
Check(await paypal.VerifyWebhookAsync(headers, eventJson.RootElement, default), "webhook verification response");
Check(calls.Last().Body?.Contains("test-webhook") == true, "webhook ID sent for verification");
var users = new SupabaseUser(http, config);
var request = new DefaultHttpContext().Request;
Check(await users.GetIdAsync(request, default) is null, "missing bearer rejected");
request.Headers.Authorization = "Bearer invalid-token-with-length";
Check(await users.GetIdAsync(request, default) is null, "invalid bearer rejected");
request.Headers.Authorization = "Bearer valid-token-with-length";
Check(await users.GetIdAsync(request, default) == Guid.Parse("11111111-1111-1111-1111-111111111111"), "Supabase validated identity");
Console.WriteLine("Payment client checks passed.");

static HttpResponseMessage Json(string content) => new(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "application/json") };
static void Check(bool condition, string name) { if (!condition) throw new Exception("Failed: " + name); }

sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> reply) : HttpMessageHandler {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => reply(request);
}
