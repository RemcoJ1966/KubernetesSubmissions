using Microsoft.Extensions.Primitives;

string? port = Environment.GetEnvironmentVariable("PORT");

if (string.IsNullOrEmpty(port))
{
    Console.WriteLine("Error: PORT environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder.Build();
app.Urls.Add($"http://0.0.0.0:{port}");

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

Console.WriteLine($"Server started in port {port}");

app.Run();