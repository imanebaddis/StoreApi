using System.Net;
using StoreApi.Infra.Repositories;
using StoreApi.Services;

var handler = new FixtureHandler();
var service = new OpenLibraryService(new HttpClient(handler) { BaseAddress = new Uri("https://openlibrary.org") });
var results = await service.SearchAsync("calvino", CancellationToken.None);
Check(results.Length == 2, "valid work count");
Check(results[0].Key == "/works/OL123W", "bare key normalization");
Check(results[0].SourceUrl == "https://openlibrary.org/works/OL123W", "source URL");
Check(service.GetSelectedBook("/works/OL123W")?.Title == "Le città invisibili", "selected metadata");
Check(service.GetSelectedBook("/books/OL123M") is null, "invalid selection rejected");
await service.SearchAsync("calvino", CancellationToken.None);
Check(handler.Calls == 1, "query cache");
var repo = new DemoBookRepository();
var imported = repo.ImportOpenLibrary(service.GetSelectedBook("/works/OL123W")!);
var again = repo.ImportOpenLibrary(service.GetSelectedBook("/works/OL123W")!);
Check(imported.Id == again.Id, "duplicate import");
Console.WriteLine("Discovery smoke tests passed.");

static void Check(bool condition, string name)
{
    if (!condition) throw new Exception("Failed: " + name);
}

sealed class FixtureHandler : HttpMessageHandler
{
    public int Calls { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        const string json = """
            {"docs":[
              {"key":"OL123W","title":"Le città invisibili","author_name":["Italo Calvino"],"first_publish_year":1972,"subject":["Fiction"]},
              {"key":"/works/OL456W","title":"Il barone rampante","author_name":["Italo Calvino"]},
              {"key":"/books/OL789M","title":"Edition","author_name":["Someone"]}
            ]}
            """;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
    }
}
