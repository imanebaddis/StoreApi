using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StoreApi.Payments;

public sealed class PayPalClient(HttpClient http, IConfiguration configuration)
{
    private string BaseUrl => configuration["PayPal:Environment"] == "Live" ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";

    private async Task<string> TokenAsync(CancellationToken ct)
    {
        var id = configuration["PayPal:ClientId"] ?? throw new InvalidOperationException("PayPal:ClientId missing");
        var secret = configuration["PayPal:ClientSecret"] ?? throw new InvalidOperationException("PayPal:ClientSecret missing");
        using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{id}:{secret}")));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "client_credentials" });
        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return json.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<JsonDocument> SendAsync(HttpMethod method, string path, object? body, string? requestId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, BaseUrl + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await TokenAsync(ct));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (requestId != null) request.Headers.Add("PayPal-Request-Id", requestId);
        if (body != null) request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request, ct);
        var content = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"PayPal returned {(int)response.StatusCode}; inspect PayPal debug logs", null, response.StatusCode);
        return JsonDocument.Parse(content);
    }

    public async Task<bool> VerifyWebhookAsync(IHeaderDictionary headers, JsonElement webhook, CancellationToken ct)
    {
        string Header(string name) => headers.TryGetValue(name, out var value) ? value.ToString() : "";
        var webhookId = configuration["PayPal:WebhookId"] ?? throw new InvalidOperationException("PayPal:WebhookId missing");
        if (new[] { Header("PAYPAL-AUTH-ALGO"), Header("PAYPAL-CERT-URL"), Header("PAYPAL-TRANSMISSION-ID"), Header("PAYPAL-TRANSMISSION-SIG"), Header("PAYPAL-TRANSMISSION-TIME") }.Any(string.IsNullOrWhiteSpace)) return false;
        // Preserve the exact event JSON received from PayPal when asking PayPal to
        // verify its signature. Reconstructing the event object can change bytes.
        var fields = new Dictionary<string, string> {
            ["auth_algo"] = Header("PAYPAL-AUTH-ALGO"), ["cert_url"] = Header("PAYPAL-CERT-URL"),
            ["transmission_id"] = Header("PAYPAL-TRANSMISSION-ID"), ["transmission_sig"] = Header("PAYPAL-TRANSMISSION-SIG"),
            ["transmission_time"] = Header("PAYPAL-TRANSMISSION-TIME"), ["webhook_id"] = webhookId
        };
        var prefix = JsonSerializer.Serialize(fields);
        var body = prefix[..^1] + ",\"webhook_event\":" + webhook.GetRawText() + "}";
        using var result = await SendRawAsync(HttpMethod.Post, "/v1/notifications/verify-webhook-signature", body, null, ct);
        return result.RootElement.GetProperty("verification_status").GetString() == "SUCCESS";
    }

    private async Task<JsonDocument> SendRawAsync(HttpMethod method, string path, string body, string? requestId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, BaseUrl + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await TokenAsync(ct));
        if (requestId != null) request.Headers.Add("PayPal-Request-Id", requestId);
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"PayPal webhook verification returned {(int)response.StatusCode}", null, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
    }
}
