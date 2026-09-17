using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using StoreApi.Infra.Models;

namespace StoreApi.Services;

public sealed record BookSearchResult(string Key, string Title, string Author, int? FirstPublishYear, string? Subject, string SourceUrl);

public sealed class OpenLibraryService
{
    private static readonly Regex WorkKey = new("^/works/OL[0-9]+W$", RegexOptions.Compiled);
    private readonly HttpClient _http;
    private readonly ConcurrentDictionary<string, (DateTimeOffset Expires, BookSearchResult[] Results)> _queries = new();
    private readonly ConcurrentDictionary<string, (DateTimeOffset Expires, BookSearchResult Result)> _selected = new();
    private readonly SemaphoreSlim _gate = new(1, 1);
    private DateTimeOffset _lastRequest = DateTimeOffset.MinValue;
    public OpenLibraryService(HttpClient http) => _http = http;

    public async Task<BookSearchResult[]> SearchAsync(string query, CancellationToken cancellationToken)
    {
        query = query.Trim();
        if (query.Length < 3 || query.Length > 100) throw new ArgumentException("La ricerca deve contenere da 3 a 100 caratteri.");
        var cacheKey = query.ToUpperInvariant();
        if (_queries.TryGetValue(cacheKey, out var cached) && cached.Expires > DateTimeOffset.UtcNow) return cached.Results;
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_queries.TryGetValue(cacheKey, out cached) && cached.Expires > DateTimeOffset.UtcNow) return cached.Results;
            var wait = _lastRequest.AddSeconds(1) - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero) await Task.Delay(wait, cancellationToken);
            _lastRequest = DateTimeOffset.UtcNow;
            using var response = await _http.GetAsync("/search.json?q=" + Uri.EscapeDataString(query) +
                "&fields=key,title,author_name,first_publish_year,subject&limit=10&lang=it", cancellationToken);
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<SearchPayload>(cancellationToken);
            var results = (payload?.Docs ?? [])
                .Select(d => new { Document = d, Key = NormalizeWorkKey(d.Key) })
                .Where(x => x.Key is not null && !string.IsNullOrWhiteSpace(x.Document.Title) && x.Document.AuthorName?.FirstOrDefault() is not null)
                .Select(x => new BookSearchResult(x.Key!, x.Document.Title!.Trim(), x.Document.AuthorName![0].Trim(), x.Document.FirstPublishYear,
                    x.Document.Subject?.FirstOrDefault(), "https://openlibrary.org" + x.Key))
                .Where(d => d.Title.Length is > 0 and <= 300 && d.Author.Length is > 0 and <= 300)
                .ToArray();
            var expiry = DateTimeOffset.UtcNow.AddMinutes(15);
            if (_queries.Count > 500)
            {
                foreach (var key in _queries.Keys.Take(100)) _queries.TryRemove(key, out _);
                foreach (var key in _selected.Keys.Take(1000))
                    if (_selected.TryGetValue(key, out var entry) && entry.Expires <= DateTimeOffset.UtcNow)
                        _selected.TryRemove(key, out _);
            }
            _queries[cacheKey] = (expiry, results);
            foreach (var result in results) _selected[result.Key] = (expiry, result);
            return results;
        }
        finally { _gate.Release(); }
    }

    public Book? GetSelectedBook(string key)
    {
        if (!WorkKey.IsMatch(key) || !_selected.TryGetValue(key, out var selected) || selected.Expires <= DateTimeOffset.UtcNow) return null;
        var result = selected.Result;
        return new Book { Title = result.Title, Author = result.Author, OpenLibraryKey = result.Key,
            FirstPublishYear = result.FirstPublishYear, Subject = result.Subject is { Length: <= 100 } ? result.Subject : null };
    }

    private static string? NormalizeWorkKey(string? key)
    {
        if (key is null) return null;
        if (Regex.IsMatch(key, "^OL[0-9]+W$")) key = "/works/" + key;
        return WorkKey.IsMatch(key) ? key : null;
    }

    private sealed class SearchPayload { [JsonPropertyName("docs")] public SearchDocument[]? Docs { get; set; } }
    private sealed class SearchDocument
    {
        [JsonPropertyName("key")] public string? Key { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("author_name")] public string[]? AuthorName { get; set; }
        [JsonPropertyName("first_publish_year")] public int? FirstPublishYear { get; set; }
        [JsonPropertyName("subject")] public string[]? Subject { get; set; }
    }
}
