using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories;

public sealed class DemoBookRepository : IBookRepository
{
    private readonly ICategoryRepository _categories;
    private readonly List<Book> _books = new()
    {
        new() { Id = 1, Title = "Il nome della rosa", Author = "Umberto Eco", CategoryId = 1 },
        new() { Id = 2, Title = "Le città invisibili", Author = "Italo Calvino", CategoryId = 2 },
        new() { Id = 3, Title = "L'amica geniale", Author = "Elena Ferrante", CategoryId = 1 }
    };
    private int _nextId = 4;

    public DemoBookRepository(ICategoryRepository categories) => _categories = categories;

    public IEnumerable<Book> GetAll(int? categoryId = null) =>
        _books.Select(WithCategory).Where(book => categoryId is null || book.CategoryId == categoryId).ToList();
    public Book? GetById(int id)
    {
        var book = _books.FirstOrDefault(item => item.Id == id);
        return book is null ? null : WithCategory(book);
    }
    public Book Add(Book book)
    {
        book.Id = _nextId++;
        _books.Add(new Book { Id = book.Id, Title = book.Title, Author = book.Author, CategoryId = book.CategoryId });
        return book;
    }
    public void Update(Book book)
    {
        var existing = _books.FirstOrDefault(item => item.Id == book.Id);
        if (existing is null) return;
        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.CategoryId = book.CategoryId;
    }
    public void Delete(int id) => _books.RemoveAll(book => book.Id == id);

    // Come "on delete set null" nel database: una categoria eliminata lascia il libro senza categoria.
    private Book WithCategory(Book book)
    {
        var category = book.CategoryId is int categoryId ? _categories.GetById(categoryId) : null;
        if (category is null) book.CategoryId = null;
        return new Book { Id = book.Id, Title = book.Title, Author = book.Author, CategoryId = category?.Id, CategoryName = category?.Name };
    }
}
