using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace StoreApi.Payments;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(PaymentStore store, PayPalClient paypal, SupabaseUser users, IConfiguration config, ILogger<PaymentsController> logger) : ControllerBase
{
    private static readonly Dictionary<string, decimal> Prices = new(StringComparer.OrdinalIgnoreCase) {
        ["Basic"] = 4m, ["Silver"] = 7m, ["Gold"] = 10m
    };

    public sealed record StartRequest(Guid RequestId, string Plan);

    [HttpPost("orders")]
    public async Task<IActionResult> Start([FromBody] StartRequest input, CancellationToken ct)
    {
        var user = await users.GetIdAsync(Request, ct);
        if (user is null) return Unauthorized();
        if (input.RequestId == Guid.Empty || !Prices.TryGetValue(input.Plan ?? "", out var price)) return BadRequest("Invalid request ID or plan");
        var plan = Prices.Keys.Single(p => p.Equals(input.Plan, StringComparison.OrdinalIgnoreCase));
        var payment = await store.CreateAsync(input.RequestId, user.Value, plan, price, ct);
        if (payment is null) return Conflict("Request ID already used");
        if (payment.Status == "completed") return Ok(new { paymentId = payment.Id, status = payment.Status });
        if (await store.HasAccessAsync(user.Value, ct)) return Conflict("An active month is already available");
        var returnUrl = config["PayPal:ReturnUrl"] ?? throw new InvalidOperationException("PayPal:ReturnUrl missing");
        var cancelUrl = config["PayPal:CancelUrl"] ?? throw new InvalidOperationException("PayPal:CancelUrl missing");
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var callback) || callback.Scheme != "https" ||
            !Uri.TryCreate(cancelUrl, UriKind.Absolute, out var cancellation) || cancellation.Scheme != "https")
            throw new InvalidOperationException("PayPal return and cancel URLs must be HTTPS");

        // The fixed price is selected server-side; the browser never supplies an amount.
        using var order = await paypal.SendAsync(HttpMethod.Post, "/v2/checkout/orders", new {
            intent = "CAPTURE",
            payment_source = new { paypal = new { experience_context = new {
                return_url = returnUrl, cancel_url = cancelUrl, user_action = "PAY_NOW"
            } } },
            purchase_units = new[] { new {
                reference_id = payment.Id.ToString("N"),
                description = $"BookStore {plan} - 1 mese, senza rinnovo automatico",
                amount = new { currency_code = "EUR", value = price.ToString("F2", CultureInfo.InvariantCulture) }
            } } }, "c-" + payment.Id.ToString("N"), ct);
        var orderId = order.RootElement.GetProperty("id").GetString()!;
        if (payment.OrderId != null && payment.OrderId != orderId) return Conflict("PayPal order mismatch");
        await store.SetOrderAsync(payment.Id, orderId, ct);
        var approve = order.RootElement.GetProperty("links").EnumerateArray()
            .FirstOrDefault(x => x.GetProperty("rel").GetString() is "approve" or "payer-action");
        var url = approve.ValueKind == JsonValueKind.Undefined ? null : approve.GetProperty("href").GetString();
        return Ok(new { paymentId = payment.Id, orderId, approvalUrl = url, status = "approval_required" });
    }

    [HttpPost("{id:guid}/capture")]
    public async Task<IActionResult> Capture(Guid id, CancellationToken ct)
    {
        var user = await users.GetIdAsync(Request, ct);
        if (user is null) return Unauthorized();
        var payment = await store.GetAsync(id, ct);
        if (payment is null || payment.UserId != user) return NotFound();
        if (payment.Status == "completed") return Ok(new { status = "completed", accessUntil = payment.AccessUntil });
        if (payment.OrderId is null) return Conflict("Order not ready");
        using var capture = await paypal.SendAsync(HttpMethod.Post, $"/v2/checkout/orders/{Uri.EscapeDataString(payment.OrderId)}/capture", new { }, "p-" + payment.Id.ToString("N"), ct);
        if (capture.RootElement.GetProperty("status").GetString() != "COMPLETED" || capture.RootElement.GetProperty("id").GetString() != payment.OrderId)
            return Conflict("Payment not completed");
        var item = capture.RootElement.GetProperty("purchase_units")[0].GetProperty("payments").GetProperty("captures")[0];
        if (item.GetProperty("status").GetString() != "COMPLETED") return Conflict("Capture not completed");
        var amount = item.GetProperty("amount");
        var value = decimal.Parse(amount.GetProperty("value").GetString()!, CultureInfo.InvariantCulture);
        var success = await store.CompleteAsync(payment.OrderId, item.GetProperty("id").GetString()!, value, amount.GetProperty("currency_code").GetString()!, ct);
        if (!success) return Conflict("Capture does not match the order");
        return Ok(new { status = "completed" });
    }

    [HttpGet("access")]
    public async Task<IActionResult> Access(CancellationToken ct)
    {
        var user = await users.GetIdAsync(Request, ct);
        return user is null ? Unauthorized() : Ok(new { active = await store.HasAccessAsync(user.Value, ct) });
    }

    [HttpPost("webhooks/paypal")]
    public async Task<IActionResult> Webhook([FromBody] JsonElement webhook, CancellationToken ct)
    {
        if (!await paypal.VerifyWebhookAsync(Request.Headers, webhook, ct)) return Unauthorized();
        var eventType = webhook.GetProperty("event_type").GetString();
        var resource = webhook.GetProperty("resource");
        if (eventType is "PAYMENT.CAPTURE.REFUNDED" or "PAYMENT.CAPTURE.REVERSED") {
            var target = PayPalWebhook.ReadRefundTarget(eventType, resource);
            if (target.IsEmpty) {
                // Never acknowledge a refund that could not be matched: a 2xx would leave access active.
                logger.LogWarning("PayPal {EventType} webhook carries no capture or order ID; access was not revoked", eventType);
                return UnprocessableEntity();
            }
            if (await store.RevokeAsync(target.CaptureIds, target.OrderId, ct)) return Ok();
            // The payment may not be recorded yet (refund delivered before completion). Let PayPal retry.
            logger.LogWarning("PayPal {EventType} webhook matches no payment yet; asking PayPal to retry", eventType);
            return StatusCode(503);
        }
        if (eventType != "PAYMENT.CAPTURE.COMPLETED") return Ok();
        if (resource.GetProperty("status").GetString() != "COMPLETED") return Ok();
        var orderId = resource.GetProperty("supplementary_data").GetProperty("related_ids").GetProperty("order_id").GetString()!;
        var amount = resource.GetProperty("amount");
        var value = decimal.Parse(amount.GetProperty("value").GetString()!, CultureInfo.InvariantCulture);
        var payment = await store.GetByOrderAsync(orderId, ct);
        if (payment is null) return StatusCode(503); // Let PayPal retry until the create request commits.
        // A refunded payment stays refunded: a late completion event must not grant access again.
        if (payment.Status is "completed" or "refunded") return Ok();
        return await store.CompleteAsync(orderId, resource.GetProperty("id").GetString()!, value, amount.GetProperty("currency_code").GetString()!, ct) ? Ok() : Conflict();
    }
}
