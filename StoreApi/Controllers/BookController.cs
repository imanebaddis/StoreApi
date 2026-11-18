using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using StoreApi.Infra.Models;
using StoreApi.Infra.DTOs;
using StoreApi.Infra.Repositories;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase {
    private readonly IBookRepository _repo;
    private readonly ICategoryRepository _categories;
    public BooksController(IBookRepository repo, ICategoryRepository categories) {
        _repo = repo;
        _categories = categories;
    }

    /// <param name="categoryId">Facoltativo: restituisce solo i libri della categoria indicata.</param>
    [HttpGet]
    public ActionResult<IEnumerable<Book>> GetAll([FromQuery] int? categoryId = null) => Ok(_repo.GetAll(categoryId));

    [HttpGet("{id}")]
    public ActionResult<Book> GetById(int id) {
        var book = _repo.GetById(id);
        return book == null ? NotFound() : Ok(book);
    }

    [HttpPost]
    [Authorize(Policy = "BookAdmin")]
    public ActionResult<Book> Create(BookDto dto) {
        if (!CategoryExists(dto.CategoryId)) return UnknownCategory();
        var book = new Book { Title = dto.Title.Trim(), Author = dto.Author.Trim(), CategoryId = dto.CategoryId };
        var created = _repo.Add(book);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, _repo.GetById(created.Id) ?? created);
    }

    // PUT sostituisce il libro: senza categoryId il libro resta senza categoria.
    [HttpPut("{id}")]
    [Authorize(Policy = "BookAdmin")]
    public IActionResult Update(int id, BookDto dto) {
        var book = _repo.GetById(id);
        if (book == null) return NotFound();
        if (!CategoryExists(dto.CategoryId)) return UnknownCategory();
        book.Title = dto.Title.Trim();
        book.Author = dto.Author.Trim();
        book.CategoryId = dto.CategoryId;
        _repo.Update(book);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "BookAdmin")]
    public IActionResult Delete(int id) {
        var book = _repo.GetById(id);
        if (book == null) return NotFound();
        _repo.Delete(id);
        return NoContent();
    }

    private bool CategoryExists(int? categoryId) => categoryId is not int id || _categories.GetById(id) != null;

    private ActionResult UnknownCategory() {
        ModelState.AddModelError(nameof(BookDto.CategoryId), "La categoria indicata non esiste.");
        return ValidationProblem(ModelState);
    }
}
