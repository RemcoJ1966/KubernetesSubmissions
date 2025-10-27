string? port = Environment.GetEnvironmentVariable("PORT");

if (string.IsNullOrEmpty(port))
{
    Console.WriteLine("Error: PORT environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder.Build();
app.Urls.Add($"http://0.0.0.0:{port}");

int count = 0;

app.MapGet("/pingpong", () => $"pong {count++}");

Console.WriteLine($"Server started in port {port}");

app.Run();