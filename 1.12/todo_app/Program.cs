string? url = Environment.GetEnvironmentVariable("LISTEN_URL");
if (string.IsNullOrEmpty(url))
{
    Console.WriteLine("Error: LISTEN_URL environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

string? delay = Environment.GetEnvironmentVariable("DELAY");
if (string.IsNullOrEmpty(delay))
{
    Console.WriteLine("Error: DELAY environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<PhotoTimerService>(sp => new PhotoTimerService(int.Parse(delay)));
builder.Services.AddHostedService(sp => sp.GetRequiredService<PhotoTimerService>());
builder.Services.AddHttpClient();

WebApplication app = builder.Build();
app.Urls.Add(url);

app.UseRouting();

app.UseStaticFiles();
app.MapRazorPages();

app.Run();

public class PhotoTimerService : BackgroundService
{
    private readonly int _delay;

    public PhotoTimerService(int delay)
    {
        _delay = delay;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_delay, cancellationToken);
            Global.NewPhoto = true;
        }
    }
}