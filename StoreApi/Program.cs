using StoreApi.Infra.Repositories;
using StoreApi.Infra.Services;
using Npgsql;
using StoreApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Configuration["PORT"] is { } portValue)
{
    if (!int.TryParse(portValue, out var port) || port is < 1 or > 65535)
        throw new InvalidOperationException("PORT must be a valid TCP port.");
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

if (args.Contains("--check-books-schema") || args.Contains("--apply-books-schema") ||
    args.Contains("--check-profiles-schema") || args.Contains("--apply-profiles-schema"))
{
    var connectionString = builder.Configuration.GetConnectionString("StoreDb")
        ?? throw new InvalidOperationException("Configure ConnectionStrings:StoreDb in user-secrets first.");
    await using var dataSource = NpgsqlDataSource.Create(connectionString);
    var profilesSchema = args.Contains("--check-profiles-schema") || args.Contains("--apply-profiles-schema");
    var tableName = profilesSchema ? "profiles" : "books";
    await using var existsCommand = dataSource.CreateCommand($"select to_regclass('store.{tableName}') is not null");
    var exists = (bool)(await existsCommand.ExecuteScalarAsync() ?? false);

    if (args.Contains("--apply-books-schema") || args.Contains("--apply-profiles-schema"))
    {
        if (exists)
            throw new InvalidOperationException($"store.{tableName} already exists; inspect it before applying a migration.");
        var scriptName = profilesSchema ? "002_profiles.sql" : "001_books.sql";
        var scriptPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "database", scriptName));
        var script = await File.ReadAllTextAsync(scriptPath);
        await using var applyCommand = dataSource.CreateCommand(script);
        await applyCommand.ExecuteNonQueryAsync();
        Console.WriteLine($"Applied database/{scriptName}.");
    }

    await using var columnsCommand = dataSource.CreateCommand(
        "select column_name, data_type from information_schema.columns " +
        $"where table_schema = 'store' and table_name = '{tableName}' order by ordinal_position");
    await using var columns = await columnsCommand.ExecuteReaderAsync();
    var found = new List<string>();
    while (await columns.ReadAsync()) found.Add($"{columns.GetString(0)} ({columns.GetString(1)})");
    Console.WriteLine(found.Count == 0 ? $"store.{tableName} is absent." : $"store.{tableName}: " + string.Join(", ", found));
    return;
}

// Registrazione servizi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var allowedOrigins = (builder.Configuration["BookStore:AllowedOrigins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
if (allowedOrigins.Length > 0)
{
    foreach (var origin in allowedOrigins)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var parsedOrigin) ||
            parsedOrigin.Scheme is not ("https" or "http") ||
            (parsedOrigin.Scheme == "http" && parsedOrigin.Host is not ("localhost" or "127.0.0.1")))
            throw new InvalidOperationException("BookStore:AllowedOrigins must contain valid HTTPS origins (or local HTTP origins).");
    }
    builder.Services.AddCors(options => options.AddPolicy("BookStoreFrontend", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
}

var supabaseUrl = builder.Configuration["Supabase:Url"]?.TrimEnd('/')
    ?? throw new InvalidOperationException("Configure Supabase:Url.");
if (!Uri.TryCreate(supabaseUrl, UriKind.Absolute, out var supabaseUri) || supabaseUri.Scheme != Uri.UriSchemeHttps)
    throw new InvalidOperationException("Supabase:Url must be an HTTPS URL.");
var issuer = $"{supabaseUrl}/auth/v1";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = issuer;
        options.Audience = "authenticated";
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ValidateLifetime = true,
            RequireSignedTokens = true,
            NameClaimType = "sub",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization(options => options.AddPolicy("BookAdmin", policy =>
    policy.RequireAuthenticatedUser().AddRequirements(new StoreAdminRequirement())));

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
    builder.Services.AddScoped<StoreProfileService>();
    builder.Services.AddScoped<IAuthorizationHandler, StoreAdminHandler>();
}
var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (allowedOrigins.Length > 0)
    app.UseCors("BookStoreFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

if (!demoMode)
{
    app.MapGet("/api/me", async (System.Security.Claims.ClaimsPrincipal user, StoreProfileService profiles,
        CancellationToken cancellationToken) =>
    {
        if (user.FindFirst("role")?.Value != "authenticated" ||
            !Guid.TryParse(user.FindFirst("sub")?.Value, out var userId))
            return Results.Unauthorized();
        var role = await profiles.GetOrCreateRoleAsync(userId, cancellationToken);
        return Results.Ok(new { userId, email = user.FindFirst("email")?.Value, role });
    }).RequireAuthorization();
}

// Mappa i controller
app.MapControllers();

app.Run();
