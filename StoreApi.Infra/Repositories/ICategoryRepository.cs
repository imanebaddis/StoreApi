using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories;

public interface ICategoryRepository
{
    IEnumerable<Category> GetAll();
    Category? GetById(int id);
    /// <exception cref="CategoryNameConflictException">Esiste già una categoria con lo stesso nome.</exception>
    Category Add(Category category);
    /// <exception cref="CategoryNameConflictException">Esiste già una categoria con lo stesso nome.</exception>
    void Update(Category category);
    void Delete(int id);
}
