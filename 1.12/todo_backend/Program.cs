using Npgsql;

string? url = Environment.GetEnvironmentVariable("LISTEN_URL");
if (string.IsNullOrEmpty(url))
{
    Console.WriteLine("Error: LISTEN_URL environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

string? connString = Environment.GetEnvironmentVariable("CONN_STRING");
if (string.IsNullOrEmpty(connString))
{
    Console.WriteLine("Error: CONN_STRING environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Services.AddNpgsqlDataSource(connString!);

WebApplication app = builder.Build();
app.Urls.Add(url);


// Ensure the table exists
NpgsqlDataSource? dataSource = app.Services.GetRequiredService<NpgsqlDataSource>();
if (dataSource is null)
{
    app.Logger.LogError("datasource not registered");
    Environment.Exit(-1); // Exit with error code -1
}

await using NpgsqlCommand createTable = dataSource.CreateCommand(
    @"CREATE TABLE IF NOT EXISTS todos (
        id BIGSERIAL PRIMARY KEY,
        todo TEXT NOT NULL)");
await createTable.ExecuteNonQueryAsync();


// Endpoints
app.MapGet("/todos", async () => 
{
    app.Logger.LogInformation("GET received at /todos");

    await using NpgsqlCommand select = dataSource.CreateCommand(
        "SELECT todo FROM todos ORDER BY id ASC");
    await using NpgsqlDataReader reader = await select.ExecuteReaderAsync();

    List<string> _todos = [];

    while (await reader.ReadAsync())
    {
        string todo = reader.GetString(0);
        app.Logger.LogInformation("Fetched todo: {}", todo);
        _todos.Add(todo);
    }

    return Results.Ok(_todos);
});

app.MapPost("/todos", async (HttpRequest request) =>
{
    app.Logger.LogInformation("POST received at /todos");
    var body = new StreamReader(request.Body);
    string todo = await body.ReadToEndAsync();

    if (todo.Length > 140)
    {
        app.Logger.LogError("Todo rejected for being too long");
        return Results.BadRequest("Todo length cannot exceed 140 characters");
    }

    await using NpgsqlCommand insert = dataSource.CreateCommand(
        $"INSERT INTO todos (todo) VALUES (\'{todo}\')");
    await insert.ExecuteNonQueryAsync();

    app.Logger.LogInformation("Stored todo: {}", todo);

    return Results.Created("/todos/{todo}", todo);
});

app.Logger.LogInformation("Server listening on {}", url);

app.Run();