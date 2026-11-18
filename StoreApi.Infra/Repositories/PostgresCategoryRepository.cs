using Npgsql;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories;

public sealed class PostgresCategoryRepository : ICategoryRepository
{
    private const string UniqueViolation = "23505";
    private readonly NpgsqlDataSource _dataSource;

    public PostgresCategoryRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public IEnumerable<Category> GetAll()
    {
        using var command = _dataSource.CreateCommand("select id, name, description from store.categories order by name");
        using var reader = command.ExecuteReader();
        var categories = new List<Category>();
        while (reader.Read()) categories.Add(ReadCategory(reader));
        return categories;
    }

    public Category? GetById(int id)
    {
        using var command = _dataSource.CreateCommand("select id, name, description from store.categories where id = @id");
        command.Parameters.AddWithValue("id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadCategory(reader) : null;
    }

    public Category Add(Category category)
    {
        using var command = _dataSource.CreateCommand(
            "insert into store.categories (name, description) values (@name, @description) returning id");
        command.Parameters.AddWithValue("name", category.Name);
        command.Parameters.AddWithValue("description", (object?)category.Description ?? DBNull.Value);
        try
        {
            category.Id = (int)(command.ExecuteScalar() ?? throw new InvalidOperationException("Category insert returned no id."));
        }
        catch (PostgresException exception) when (exception.SqlState == UniqueViolation)
        {
            throw new CategoryNameConflictException(category.Name, exception);
        }
        return category;
    }

    public void Update(Category category)
    {
        using var command = _dataSource.CreateCommand(
            "update store.categories set name = @name, description = @description where id = @id");
        command.Parameters.AddWithValue("id", category.Id);
        command.Parameters.AddWithValue("name", category.Name);
        command.Parameters.AddWithValue("description", (object?)category.Description ?? DBNull.Value);
        try
        {
            command.ExecuteNonQuery();
        }
        catch (PostgresException exception) when (exception.SqlState == UniqueViolation)
        {
            throw new CategoryNameConflictException(category.Name, exception);
        }
    }

    public void Delete(int id)
    {
        // La chiave esterna "on delete set null" lascia i libri senza categoria.
        using var command = _dataSource.CreateCommand("delete from store.categories where id = @id");
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    private static Category ReadCategory(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Description = reader.IsDBNull(2) ? null : reader.GetString(2)
    };
}
