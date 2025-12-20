using Npgsql;

string? connString = Environment.GetEnvironmentVariable("CONN_STRING");
if (string.IsNullOrEmpty(connString))
{
    Console.WriteLine("Error: CONN_STRING environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(connString!);
if (dataSource is null)
{
    Console.WriteLine("Error: datasource not registered.");
    Environment.Exit(-1); // Exit with error code -1
}

// Ensure the table exists
await using NpgsqlCommand createTable = dataSource.CreateCommand(
    @"CREATE TABLE IF NOT EXISTS todos (
        id BIGSERIAL PRIMARY KEY,
        todo TEXT NOT NULL)");
await createTable.ExecuteNonQueryAsync();


// Get the random link and store it in the database
string link = "https://en.wikipedia.org/wiki/Special:Random";
using HttpClient client = new();
HttpResponseMessage response = await client.GetAsync(link);

if (response.Headers.Location is not null)
{
    Uri randomUri = response.Headers.Location;
    Console.WriteLine($"Retrieved random link {randomUri}");

    await using NpgsqlCommand insert = dataSource.CreateCommand(
        $"INSERT INTO todos (todo) VALUES (\'Read {randomUri}\')");
    await insert.ExecuteNonQueryAsync();
}
else
{
    Console.WriteLine("Error: could not retrieve random link.");

    await using NpgsqlCommand insert = dataSource.CreateCommand(
        $"INSERT INTO todos (todo) VALUES (\'Figure out why the wikipedia link returns {response.StatusCode}\')");
    await insert.ExecuteNonQueryAsync();

    Environment.Exit(-1); // Exit with error code -1
}
