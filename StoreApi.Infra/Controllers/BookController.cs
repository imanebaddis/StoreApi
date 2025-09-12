using Microsoft.AspNetCore.Mvc;

using StoreApi.Infra.Models;
using StoreApi.Infra.DTOs;
using StoreApi.Infra.Repositories;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase {
    private readonly IBookRepository _repo;
    public BooksController(IBookRepository repo) => _repo = repo;

    [HttpGet]
    public ActionResult<IEnumerable<Book>> GetAll() => Ok(_repo.GetAll());

    [HttpGet("{id}")]
    public ActionResult<Book> GetById(int id) {
        var book = _repo.GetById(id);
        return book == null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public ActionResult<Book> Create(BookDto dto) {
        var book = new Book { Title = dto.Title, Author = dto.Author };
        var created = _repo.Add(book);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, BookDto dto) {
        var book = _repo.GetById(id);
        if (book == null) return NotFound();
        book.Title = dto.Title;
        book.Author = dto.Author;
        _repo.Update(book);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id) {
        var book = _repo.GetById(id);
        if (book == null) return NotFound();
        _repo.Delete(id);
        return NoContent();
    }
}