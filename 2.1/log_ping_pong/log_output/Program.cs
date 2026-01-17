using System.Globalization;
using System.Text;

string? port = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrEmpty(port))
{
    Console.WriteLine("Error: PORT environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

string? message = Environment.GetEnvironmentVariable("MESSAGE");
if (string.IsNullOrEmpty(port))
{
    Console.WriteLine("Error: MESSAGE environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<RandomStringService>();
builder.Services.AddHostedService<RandomStringService>(sp => sp.GetRequiredService<RandomStringService>());


WebApplication app = builder.Build();
app.Urls.Add($"http://0.0.0.0:{port}");

app.MapGet("/", async (RandomStringService service) => Results.Content(await ComposeHtml(service.Status), "text/html"));

Console.WriteLine($"Server started in port {port}");

app.Run();

async Task<string> ComposeHtml(string status)
{
    StringBuilder strBuilder = new(FileContentsToHtml());

    strBuilder.Append($"<div><p>env variable: MESSAGE={message}</p></div>");
    strBuilder.Append($"<div><p>{status}</p></div>");
    strBuilder.Append($"<div><p>Ping / Pongs: {await GetPingPongCount("http://logpingpong-svc:2345/pings")}</p></div>");

    return strBuilder.ToString();
}

string FileContentsToHtml()
{
    string path = @"/var/tmp/information.txt";

    StringBuilder strBuilder = new("<div>");

    using StreamReader reader = new(path);
    string? line;
    while ((line = reader.ReadLine()) is not null)
    {
        strBuilder.Append($"<p>file content: {line}</p>");
    }

    strBuilder.Append("</div>");

    return strBuilder.ToString();
}

async Task<string> GetPingPongCount(string pingpongUrl)
{
    using HttpClient client = new();
    HttpResponseMessage response = await client.GetAsync(pingpongUrl);
    if (response.IsSuccessStatusCode)
    {
        return await response.Content.ReadAsStringAsync();
    }
    else
    {
        return "Failed to retrieve pingpong count";
    }
}

public class RandomStringService : BackgroundService
{
    private static readonly string formatString =
        CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern;
    private static readonly string randomStr = Guid.NewGuid().ToString();
    public string Status { get; private set; } = string.Empty;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Status = $"{DateTime.Now.ToString(formatString)}: {randomStr}";
            Console.WriteLine(Status);
            await Task.Delay(5000, cancellationToken);
        }
    }
}
