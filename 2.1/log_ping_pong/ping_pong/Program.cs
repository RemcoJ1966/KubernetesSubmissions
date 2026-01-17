using Npgsql;

string? port = Environment.GetEnvironmentVariable("PORT");

if (string.IsNullOrEmpty(port))
{
    Console.WriteLine("Error: PORT environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Services.AddNpgsqlDataSource(
    "Host=postgres-svc.default;Port=5432;Username=postgres;Password=example");

WebApplication app = builder.Build();
app.Urls.Add($"http://0.0.0.0:{port}");

int count = 0;

// Ensure the table exists
NpgsqlDataSource? dataSource = app.Services.GetRequiredService<NpgsqlDataSource>();
if (dataSource is null)
{
    Console.WriteLine("Error: datasource not registered.");
    Environment.Exit(-1); // Exit with error code -1
}

try
{
    await using NpgsqlCommand createTable = dataSource.CreateCommand(
        "CREATE TABLE IF NOT EXISTS pings (ping_count INTEGER)");
    await createTable.ExecuteNonQueryAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Couldn't create table: {ex.Message}");
}


// Endpoints
app.MapGet("/", async (NpgsqlDataSource dataSource) => 
{
    count++;

    try
    {
        await using NpgsqlCommand insert = dataSource.CreateCommand(
            $"INSERT INTO pings (ping_count) VALUES ({count})");
        await insert.ExecuteNonQueryAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Couldn't insert into table: {ex.Message}");
    }

    return $"pong {count}";
});

app.MapGet("/pings", () => $"{count}");

// Run app
Console.WriteLine($"Server started in port {port}");

app.Run();