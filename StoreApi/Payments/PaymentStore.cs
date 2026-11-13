using Npgsql;

namespace StoreApi.Payments;

public sealed record Payment(Guid Id, Guid UserId, string Plan, decimal Amount, string Status, string? OrderId, string? CaptureId, DateTime? AccessUntil);

public sealed class PaymentStore(NpgsqlDataSource db)
{
    public async Task<Payment?> CreateAsync(Guid id, Guid userId, string plan, decimal amount, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("insert into store.payments(id,user_id,plan,amount,currency,status) values (@id,@user,@plan,@amount,'EUR','pending') on conflict (id) do nothing");
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("user", userId);
        cmd.Parameters.AddWithValue("plan", plan);
        cmd.Parameters.AddWithValue("amount", amount);
        await cmd.ExecuteNonQueryAsync(ct);
        var existing = await GetAsync(id, ct);
        return existing?.UserId == userId && existing.Plan == plan && existing.Amount == amount ? existing : null;
    }

    public async Task<Payment?> GetAsync(Guid id, CancellationToken ct) => await FindAsync("id", id, ct);
    public async Task<Payment?> GetByOrderAsync(string orderId, CancellationToken ct) => await FindAsync("paypal_order_id", orderId, ct);

    private async Task<Payment?> FindAsync(string column, object value, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand($"select id,user_id,plan,amount,status,paypal_order_id,paypal_capture_id,access_until from store.payments where {column}=@value");
        cmd.Parameters.AddWithValue("value", value);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? new Payment(reader.GetGuid(0), reader.GetGuid(1), reader.GetString(2), reader.GetDecimal(3), reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.IsDBNull(7) ? null : reader.GetDateTime(7)) : null;
    }

    public async Task SetOrderAsync(Guid id, string orderId, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("update store.payments set paypal_order_id=@order,status='approved' where id=@id and status='pending'");
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("order", orderId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> CompleteAsync(string orderId, string captureId, decimal amount, string currency, CancellationToken ct)
    {
        await using var connection = await db.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using var cmd = new NpgsqlCommand("update store.payments set status='completed',paypal_capture_id=@capture,completed_at=now(),access_until=now()+interval '1 month' where paypal_order_id=@order and status in ('pending','approved') and amount=@amount and currency=@currency", connection, transaction);
        cmd.Parameters.AddWithValue("capture", captureId);
        cmd.Parameters.AddWithValue("order", orderId);
        cmd.Parameters.AddWithValue("amount", amount);
        cmd.Parameters.AddWithValue("currency", currency);
        var updated = await cmd.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
        return updated == 1;
    }

    public async Task<bool> HasAccessAsync(Guid userId, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("select exists(select 1 from store.payments where user_id=@user and status='completed' and access_until > now())");
        cmd.Parameters.AddWithValue("user", userId);
        return (bool)(await cmd.ExecuteScalarAsync(ct) ?? false);
    }

    public async Task RevokeByCaptureAsync(string captureId, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("update store.payments set status='refunded',access_until=now() where paypal_capture_id=@capture and status='completed'");
        cmd.Parameters.AddWithValue("capture", captureId);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
