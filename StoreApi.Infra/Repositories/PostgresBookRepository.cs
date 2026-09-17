using Npgsql;
using NpgsqlTypes;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories;

public sealed class PostgresBookRepository : IBookRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresBookRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public IEnumerable<Book> GetAll()
    {
        using var command = _dataSource.CreateCommand("select id, title, author, open_library_key, first_publish_year, subject from store.books order by id");
        using var reader = command.ExecuteReader();
        var books = new List<Book>();
        while (reader.Read()) books.Add(ReadBook(reader));
        return books;
    }

    public Book? GetById(int id)
    {
        using var command = _dataSource.CreateCommand("select id, title, author, open_library_key, first_publish_year, subject from store.books where id = @id");
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

    public Book ImportOpenLibrary(Book book)
    {
        using var command = _dataSource.CreateCommand("""
            insert into store.books (title, author, open_library_key, first_publish_year, subject)
            values (@title, @author, @key, @year, @subject)
            on conflict do nothing
            returning id, title, author, open_library_key, first_publish_year, subject
            """);
        command.Parameters.AddWithValue("title", book.Title);
        command.Parameters.AddWithValue("author", book.Author);
        command.Parameters.AddWithValue("key", book.OpenLibraryKey!);
        command.Parameters.Add("year", NpgsqlDbType.Integer).Value = (object?)book.FirstPublishYear ?? DBNull.Value;
        command.Parameters.Add("subject", NpgsqlDbType.Text).Value = (object?)book.Subject ?? DBNull.Value;
        using (var reader = command.ExecuteReader())
            if (reader.Read()) return ReadBook(reader);
        using var existing = _dataSource.CreateCommand("""
            select id, title, author, open_library_key, first_publish_year, subject from store.books
            where open_library_key = @key or (lower(trim(title)) = lower(trim(@title)) and lower(trim(author)) = lower(trim(@author)))
            limit 1
            """);
        existing.Parameters.AddWithValue("key", book.OpenLibraryKey!);
        existing.Parameters.AddWithValue("title", book.Title);
        existing.Parameters.AddWithValue("author", book.Author);
        using var found = existing.ExecuteReader();
        return found.Read() ? ReadBook(found) : throw new InvalidOperationException("Import conflict without matching book.");
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
        Author = reader.GetString(2),
        OpenLibraryKey = reader.IsDBNull(3) ? null : reader.GetString(3),
        FirstPublishYear = reader.IsDBNull(4) ? null : reader.GetInt32(4),
        Subject = reader.IsDBNull(5) ? null : reader.GetString(5)
    };
}
