using System.Text.Json;

namespace StoreApi.Payments;

public static class PayPalWebhook
{
    public sealed record RefundTarget(IReadOnlyList<string> CaptureIds, string? OrderId)
    {
        public bool IsEmpty => CaptureIds.Count == 0 && OrderId is null;
    }

    // A refund or reversal must find the payment it cancels. PayPal does not always
    // send the capture ID in the same place, so every known location is read.
    public static RefundTarget ReadRefundTarget(string? eventType, JsonElement resource)
    {
        var captures = new List<string>();
        string? orderId = null;

        if (resource.ValueKind != JsonValueKind.Object) return new RefundTarget(captures, null);

        if (resource.TryGetProperty("supplementary_data", out var extra) && extra.ValueKind == JsonValueKind.Object &&
            extra.TryGetProperty("related_ids", out var related) && related.ValueKind == JsonValueKind.Object)
        {
            Add(captures, Text(related, "capture_id"));
            orderId = Text(related, "order_id");
        }

        if (resource.TryGetProperty("links", out var links) && links.ValueKind == JsonValueKind.Array)
        {
            foreach (var link in links.EnumerateArray())
            {
                if (link.ValueKind != JsonValueKind.Object || Text(link, "rel") != "up") continue;
                Add(captures, CaptureIdFromHref(Text(link, "href")));
            }
        }

        // In a reversal the resource can be the capture itself.
        if (eventType == "PAYMENT.CAPTURE.REVERSED") Add(captures, Text(resource, "id"));

        return new RefundTarget(captures, orderId);
    }

    private static string? CaptureIdFromHref(string? href)
    {
        const string marker = "/captures/";
        if (href is null || !Uri.TryCreate(href, UriKind.Absolute, out var uri)) return null;
        var start = uri.AbsolutePath.LastIndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return null;
        var id = uri.AbsolutePath[(start + marker.Length)..].Split('/')[0];
        return Uri.UnescapeDataString(id);
    }

    private static string? Text(JsonElement parent, string name) =>
        parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static void Add(List<string> ids, string? id)
    {
        if (!string.IsNullOrWhiteSpace(id) && !ids.Contains(id)) ids.Add(id);
    }
}
