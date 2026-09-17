using StoreApi.Infra.Repositories;
using StoreApi.Infra.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (args.Contains("--check-books-schema") || args.Contains("--apply-books-schema"))
{
    var connectionString = builder.Configuration.GetConnectionString("StoreDb")
        ?? throw new InvalidOperationException("Configure ConnectionStrings:StoreDb in user-secrets first.");
    await using var dataSource = NpgsqlDataSource.Create(connectionString);
    await using var existsCommand = dataSource.CreateCommand("select to_regclass('store.books') is not null");
    var exists = (bool)(await existsCommand.ExecuteScalarAsync() ?? false);

    if (args.Contains("--apply-books-schema"))
    {
        if (exists)
            throw new InvalidOperationException("store.books already exists; inspect it before applying a migration.");
        var scriptPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "database", "001_books.sql"));
        var script = await File.ReadAllTextAsync(scriptPath);
        await using var applyCommand = dataSource.CreateCommand(script);
        await applyCommand.ExecuteNonQueryAsync();
        Console.WriteLine("Applied database/001_books.sql.");
    }

    await using var columnsCommand = dataSource.CreateCommand(
        "select column_name, data_type from information_schema.columns " +
        "where table_schema = 'store' and table_name = 'books' order by ordinal_position");
    await using var columns = await columnsCommand.ExecuteReaderAsync();
    var found = new List<string>();
    while (await columns.ReadAsync()) found.Add($"{columns.GetString(0)} ({columns.GetString(1)})");
    Console.WriteLine(found.Count == 0 ? "store.books is absent." : "store.books: " + string.Join(", ", found));
    return;
}

// Registrazione servizi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repositories
var demoMode = builder.Configuration.GetValue<bool>("BookStore:DemoMode");
if (demoMode)
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("BookStore:DemoMode is available only in Development.");
    builder.Services.AddSingleton<IBookRepository, DemoBookRepository>();
}
else
{
    var databaseConnection = builder.Configuration.GetConnectionString("StoreDb");
    if (string.IsNullOrWhiteSpace(databaseConnection))
        throw new InvalidOperationException("Set ConnectionStrings__StoreDb to a Supabase Postgres connection string.");
    if (databaseConnection.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("ConnectionStrings__StoreDb must use Npgsql's Host=...;Database=... format.");

    builder.Services.AddSingleton(NpgsqlDataSource.Create(databaseConnection));
    builder.Services.AddScoped<IBookRepository, PostgresBookRepository>();
}
builder.Services.AddSingleton<ISubscriptionRepository, InMemorySubscriptionRepository>();
builder.Services.AddSingleton<IBankCardRepository, InMemoryBankCardRepository>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// Register services
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IBankCardService, BankCardService>();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint minimal API (facoltativo, può coesistere con i controller)
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Mappa i controller
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
