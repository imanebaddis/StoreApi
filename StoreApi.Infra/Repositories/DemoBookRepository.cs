using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories;

public sealed class DemoBookRepository : IBookRepository
{
    private readonly List<Book> _books = new()
    {
        new() { Id = 1, Title = "Il nome della rosa", Author = "Umberto Eco" },
        new() { Id = 2, Title = "Le città invisibili", Author = "Italo Calvino" },
        new() { Id = 3, Title = "L'amica geniale", Author = "Elena Ferrante" }
    };
    private int _nextId = 4;

    public IEnumerable<Book> GetAll() => _books.ToList();
    public Book? GetById(int id) => _books.FirstOrDefault(book => book.Id == id);
    public Book Add(Book book)
    {
        book.Id = _nextId++;
        _books.Add(book);
        return book;
    }
    public void Update(Book book)
    {
        var existing = GetById(book.Id);
        if (existing is null) return;
        existing.Title = book.Title;
        existing.Author = book.Author;
    }
    public void Delete(int id)
    {
        var book = GetById(id);
        if (book is not null) _books.Remove(book);
    }
}
