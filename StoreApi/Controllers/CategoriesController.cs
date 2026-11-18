using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using StoreApi.Infra.Models;
using StoreApi.Infra.DTOs;
using StoreApi.Infra.Repositories;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase {
    private readonly ICategoryRepository _repo;
    public CategoriesController(ICategoryRepository repo) => _repo = repo;

    [HttpGet]
    public ActionResult<IEnumerable<Category>> GetAll() => Ok(_repo.GetAll());

    [HttpGet("{id}")]
    public ActionResult<Category> GetById(int id) {
        var category = _repo.GetById(id);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [Authorize(Policy = "BookAdmin")]
    public ActionResult<Category> Create(CategoryDto dto) {
        var category = FromDto(dto);
        try {
            var created = _repo.Add(category);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        } catch (CategoryNameConflictException exception) {
            return Conflict(new ProblemDetails { Title = "Categoria già esistente", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "BookAdmin")]
    public IActionResult Update(int id, CategoryDto dto) {
        if (_repo.GetById(id) == null) return NotFound();
        var category = FromDto(dto);
        category.Id = id;
        try {
            _repo.Update(category);
        } catch (CategoryNameConflictException exception) {
            return Conflict(new ProblemDetails { Title = "Categoria già esistente", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
        return NoContent();
    }

    // I libri della categoria eliminata restano nel catalogo senza categoria.
    [HttpDelete("{id}")]
    [Authorize(Policy = "BookAdmin")]
    public IActionResult Delete(int id) {
        if (_repo.GetById(id) == null) return NotFound();
        _repo.Delete(id);
        return NoContent();
    }

    private static Category FromDto(CategoryDto dto) => new() {
        Name = dto.Name.Trim(),
        Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
    };
}
