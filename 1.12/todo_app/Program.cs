string? port = Environment.GetEnvironmentVariable("PORT");

if (string.IsNullOrEmpty(port))
{
    Console.WriteLine("Error: PORT environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<PhotoTimerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<PhotoTimerService>());
builder.Services.AddHttpClient();

WebApplication app = builder.Build();
app.Urls.Add($"http://0.0.0.0:{port}");

app.UseRouting();

app.UseStaticFiles();
app.MapRazorPages();

app.Run();

public class PhotoTimerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(600000, cancellationToken);
            Global.NewPhoto = true;
        }
    }
}