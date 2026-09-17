using Npgsql;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories;

public sealed class PostgresBookRepository : IBookRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresBookRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public IEnumerable<Book> GetAll()
    {
        using var command = _dataSource.CreateCommand("select id, title, author from store.books order by id");
        using var reader = command.ExecuteReader();
        var books = new List<Book>();
        while (reader.Read()) books.Add(ReadBook(reader));
        return books;
    }

    public Book? GetById(int id)
    {
        using var command = _dataSource.CreateCommand("select id, title, author from store.books where id = @id");
        command.Parameters.AddWithValue("id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadBook(reader) : null;
    }

    public Book Add(Book book)
    {
        using var command = _dataSource.CreateCommand("insert into store.books (title, author) values (@title, @author) returning id");
        command.Parameters.AddWithValue("title", book.Title);
        command.Parameters.AddWithValue("author", book.Author);
        book.Id = (int)(command.ExecuteScalar() ?? throw new InvalidOperationException("Book insert returned no id."));
        return book;
    }

    public void Update(Book book)
    {
        using var command = _dataSource.CreateCommand("update store.books set title = @title, author = @author where id = @id");
        command.Parameters.AddWithValue("id", book.Id);
        command.Parameters.AddWithValue("title", book.Title);
        command.Parameters.AddWithValue("author", book.Author);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var command = _dataSource.CreateCommand("delete from store.books where id = @id");
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    private static Book ReadBook(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Title = reader.GetString(1),
        Author = reader.GetString(2)
    };
}
