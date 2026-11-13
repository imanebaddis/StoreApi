using System.Net.Http.Headers;
using System.Text.Json;

namespace StoreApi.Payments;

public sealed class SupabaseUser(HttpClient http, IConfiguration configuration)
{
    public async Task<Guid?> GetIdAsync(HttpRequest request, CancellationToken ct)
    {
        var bearer = request.Headers.Authorization.ToString();
        if (!bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null;
        var token = bearer[7..].Trim();
        if (token.Length is < 20 or > 8192) return null;
        var url = configuration["Supabase:Url"]?.TrimEnd('/') ?? throw new InvalidOperationException("Supabase:Url missing");
        var key = configuration["Supabase:PublishableKey"] ?? throw new InvalidOperationException("Supabase:PublishableKey missing");
        using var authRequest = new HttpRequestMessage(HttpMethod.Get, url + "/auth/v1/user");
        authRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        authRequest.Headers.Add("apikey", key);
        using var response = await http.SendAsync(authRequest, ct);
        if (!response.IsSuccessStatusCode) return null;
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return Guid.TryParse(json.RootElement.GetProperty("id").GetString(), out var id) ? id : null;
    }
}
