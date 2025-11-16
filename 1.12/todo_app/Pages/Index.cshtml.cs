using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace todo_app.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    [BindProperty]
    public string PhotoFile { get; init; } = "images/photo.jpg";

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;

        // Don't get a new photo if one exists from a previous run
        if (System.IO.File.Exists($"wwwroot/{PhotoFile}"))
        {
            _logger.LogInformation("Reusing existing photo from previous run");
            Global.NewPhoto = false;
        }
    }

    public async Task OnGetAsync()
    {
        if (Global.NewPhoto)
        {
            _logger.LogInformation("Downloading a new photo");
            await DownloadImageAsync("https://picsum.photos/1200", $"wwwroot/{PhotoFile}");
            Global.NewPhoto = false;
        }
    }

    public async Task DownloadImageAsync(string imageUrl, string savePath)
    {
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(imageUrl);
        response.EnsureSuccessStatusCode();

        string? directory = Path.GetDirectoryName(savePath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Byte[] imageData = await response.Content.ReadAsByteArrayAsync();
        await System.IO.File.WriteAllBytesAsync(savePath, imageData);
    }
}

