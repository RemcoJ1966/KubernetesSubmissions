string? url = Environment.GetEnvironmentVariable("LISTEN_URL");
if (string.IsNullOrEmpty(url))
{
    Console.WriteLine("Error: LISTEN_URL environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder.Build();
app.Urls.Add(url);

List<string> _todos = [];

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
    return Results.Created("/todos/{todo}", todo);
});

Console.WriteLine($"Server listening on {url}");

app.Run();