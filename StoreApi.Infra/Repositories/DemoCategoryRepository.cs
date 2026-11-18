using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories;

public sealed class DemoCategoryRepository : ICategoryRepository
{
    private readonly object _sync = new();
    private readonly List<Category> _categories = new()
    {
        new() { Id = 1, Name = "Narrativa", Description = "Romanzi e racconti." },
        new() { Id = 2, Name = "Classici", Description = "Opere che hanno attraversato le generazioni." },
        new() { Id = 3, Name = "Saggi", Description = "Riflessioni, storia e idee." },
        new() { Id = 4, Name = "Scienza", Description = "Divulgazione scientifica." }
    };
    private int _nextId = 5;

    public IEnumerable<Category> GetAll()
    {
        lock (_sync)
            return _categories.OrderBy(category => category.Name, StringComparer.CurrentCultureIgnoreCase).Select(Copy).ToList();
    }

    public Category? GetById(int id)
    {
        lock (_sync)
        {
            var category = _categories.FirstOrDefault(item => item.Id == id);
            return category is null ? null : Copy(category);
        }
    }

    public Category Add(Category category)
    {
        lock (_sync)
        {
            EnsureUniqueName(category.Name, null);
            category.Id = _nextId++;
            _categories.Add(Copy(category));
            return category;
        }
    }

    public void Update(Category category)
    {
        lock (_sync)
        {
            var existing = _categories.FirstOrDefault(item => item.Id == category.Id);
            if (existing is null) return;
            EnsureUniqueName(category.Name, category.Id);
            existing.Name = category.Name;
            existing.Description = category.Description;
        }
    }

    public void Delete(int id)
    {
        lock (_sync)
            _categories.RemoveAll(category => category.Id == id);
    }

    private void EnsureUniqueName(string name, int? excludedId)
    {
        var key = name.Trim();
        if (_categories.Any(item => item.Id != excludedId &&
                string.Equals(item.Name.Trim(), key, StringComparison.OrdinalIgnoreCase)))
            throw new CategoryNameConflictException(key);
    }

    private static Category Copy(Category category) =>
        new() { Id = category.Id, Name = category.Name, Description = category.Description };
}
