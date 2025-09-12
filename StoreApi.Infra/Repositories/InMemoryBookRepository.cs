using System.Collections.Generic;
using System.Linq;
using StoreApi.Infra.Models;
namespace StoreApi.Infra.Repositories {
    public class InMemoryBookRepository : IBookRepository {
        private readonly List<Book> _books = new();
        private int _nextId = 1;
        public IEnumerable<Book> GetAll() => _books;
        public Book GetById(int id) => _books.FirstOrDefault(b => b.Id == id);
        public Book Add(Book book) {
            book.Id = _nextId++;
            _books.Add(book);
            return book;
        }
        public void Update(Book book) {
            var existing = GetById(book.Id);
            if (existing != null) {
                existing.Title = book.Title;
                existing.Author = book.Author;
            }
        }
        public void Delete(int id) {
            var book = GetById(id);
            if (book != null) _books.Remove(book);
        }
    }
}