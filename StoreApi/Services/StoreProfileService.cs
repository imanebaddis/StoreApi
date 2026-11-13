using Npgsql;

namespace StoreApi.Services;

public sealed class StoreProfileService
{
    private readonly NpgsqlDataSource _dataSource;

    public StoreProfileService(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<string?> GetOrCreateRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using (var insert = _dataSource.CreateCommand(
            "insert into store.profiles (user_id) values (@user_id) on conflict (user_id) do nothing"))
        {
            insert.Parameters.AddWithValue("user_id", userId);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var select = _dataSource.CreateCommand(
            "select role from store.profiles where user_id = @user_id");
        select.Parameters.AddWithValue("user_id", userId);
        return (string?)await select.ExecuteScalarAsync(cancellationToken);
    }
}
