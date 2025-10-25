using System.Globalization;

string? port = Environment.GetEnvironmentVariable("PORT");

if (string.IsNullOrEmpty(port))
{
    Console.WriteLine("Error: PORT environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RandomStringService>();
builder.Services.AddHostedService<RandomStringService>(sp => sp.GetRequiredService<RandomStringService>());

WebApplication app = builder.Build();
app.Urls.Add($"http://0.0.0.0:{port}");

app.MapGet("/", () => Results.Content($"<div><h1>Server started in port {port}</h1></div><div><p><a href=\"/status\">status</a></div>", "text/html"));
app.MapGet("/status", (RandomStringService service) => Results.Content($"<div><h1>{service.Status}</h1></div><div><a href=\"/\">home</a></div>", "text/html"));

Console.WriteLine($"Server started in port {port}");

app.Run();

public class RandomStringService : BackgroundService
{
    private static readonly string formatString =
        CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern;
    private static readonly string randomStr = Guid.NewGuid().ToString();
    public string Status => $"{DateTime.Now.ToString(formatString)}: {randomStr}";

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine(Status);
            await Task.Delay(5000, cancellationToken);
        }
    }
}