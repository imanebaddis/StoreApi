using Microsoft.AspNetCore.Mvc;
using StoreApi.Infra.Repositories;
using StoreApi.Services;

namespace StoreApi.Controllers;

[ApiController]
[Route("api/discovery")]
public sealed class DiscoveryController : ControllerBase
{
    private readonly OpenLibraryService _openLibrary;
    private readonly IBookRepository _books;
    private readonly IWebHostEnvironment _environment;
    public DiscoveryController(OpenLibraryService openLibrary, IBookRepository books, IWebHostEnvironment environment)
    { _openLibrary = openLibrary; _books = books; _environment = environment; }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken cancellationToken)
    {
        try { return Ok(await _openLibrary.SearchAsync(q ?? "", cancellationToken)); }
        catch (ArgumentException error) { return BadRequest(new { error = error.Message }); }
        catch (HttpRequestException) { return StatusCode(503, new { error = "Ricerca esterna temporaneamente non disponibile." }); }
    }

    [HttpPost("import")]
    public IActionResult Import([FromBody] ImportRequest request)
    {
        if (!_environment.IsDevelopment()) return StatusCode(403, new { error = "Importazione disponibile solo nell'ambiente di sviluppo finché non è integrato il controllo di accesso." });
        var book = _openLibrary.GetSelectedBook(request.Key ?? "");
        if (book is null) return BadRequest(new { error = "Cerca di nuovo il libro e seleziona un risultato valido." });
        return Ok(_books.ImportOpenLibrary(book));
    }

    public sealed record ImportRequest(string? Key);
}
