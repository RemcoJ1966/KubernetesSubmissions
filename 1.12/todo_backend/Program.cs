using Npgsql;

string? url = Environment.GetEnvironmentVariable("LISTEN_URL");
if (string.IsNullOrEmpty(url))
{
    Console.WriteLine("Error: LISTEN_URL environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

string? connString = Environment.GetEnvironmentVariable("CONN_STRING");
if (string.IsNullOrEmpty(url))
{
    Console.WriteLine("Error: CONN_STRING environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Services.AddNpgsqlDataSource(connString!);

WebApplication app = builder.Build();
app.Urls.Add(url);

List<string> _todos = [];


// Ensure the table exists
NpgsqlDataSource? dataSource = app.Services.GetRequiredService<NpgsqlDataSource>();
if (dataSource is null)
{
    Console.WriteLine("Error: datasource not registered.");
    Environment.Exit(-1); // Exit with error code -1
}

await using NpgsqlCommand createTable = dataSource.CreateCommand(
    @"CREATE TABLE IF NOT EXISTS todos (
        id BIGSERIAL PRIMARY KEY,
        todo TEXT NOT NULL)");
await createTable.ExecuteNonQueryAsync();


// Endpoints
app.MapGet("/todos", () => 
{
    Console.WriteLine("GET received at /todos");
    return Results.Ok(_todos);
});

app.MapPost("/todos", async (HttpRequest request) =>
{
    Console.WriteLine("POST received at /todos");
    var body = new StreamReader(request.Body);
    string todo = await body.ReadToEndAsync();

    _todos.Add(todo);

    await using NpgsqlCommand insert = dataSource.CreateCommand(
        $"INSERT INTO todos (todo) VALUES (\'{todo}\')");
    await insert.ExecuteNonQueryAsync();

    return Results.Created("/todos/{todo}", todo);
});

Console.WriteLine($"Server listening on {url}");

app.Run();