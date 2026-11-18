using Npgsql;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories;

public sealed class PostgresBookRepository : IBookRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresBookRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    private const string SelectBooks =
        "select b.id, b.title, b.author, b.category_id, c.name from store.books b " +
        "left join store.categories c on c.id = b.category_id";

    public IEnumerable<Book> GetAll(int? categoryId = null)
    {
        using var command = _dataSource.CreateCommand(categoryId is null
            ? $"{SelectBooks} order by b.id"
            : $"{SelectBooks} where b.category_id = @category_id order by b.id");
        if (categoryId is not null) command.Parameters.AddWithValue("category_id", categoryId.Value);
        using var reader = command.ExecuteReader();
        var books = new List<Book>();
        while (reader.Read()) books.Add(ReadBook(reader));
        return books;
    }

    public Book? GetById(int id)
    {
        using var command = _dataSource.CreateCommand($"{SelectBooks} where b.id = @id");
        command.Parameters.AddWithValue("id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadBook(reader) : null;
    }

    public Book Add(Book book)
    {
        using var command = _dataSource.CreateCommand("insert into store.books (title, author, category_id) values (@title, @author, @category_id) returning id");
        command.Parameters.AddWithValue("title", book.Title);
        command.Parameters.AddWithValue("author", book.Author);
        command.Parameters.AddWithValue("category_id", (object?)book.CategoryId ?? DBNull.Value);
        book.Id = (int)(command.ExecuteScalar() ?? throw new InvalidOperationException("Book insert returned no id."));
        return book;
    }

    public void Update(Book book)
    {
        using var command = _dataSource.CreateCommand("update store.books set title = @title, author = @author, category_id = @category_id where id = @id");
        command.Parameters.AddWithValue("id", book.Id);
        command.Parameters.AddWithValue("title", book.Title);
        command.Parameters.AddWithValue("author", book.Author);
        command.Parameters.AddWithValue("category_id", (object?)book.CategoryId ?? DBNull.Value);
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
        Author = reader.GetString(2),
        CategoryId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
        CategoryName = reader.IsDBNull(4) ? null : reader.GetString(4)
    };
}
