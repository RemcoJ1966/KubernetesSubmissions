using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace todo_app.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty]
    public string PhotoFile { get; init; } = "images/photo.jpg";

    [BindProperty]
    public string Todo { get; set; } = "";

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public List<string> _todos { get; set; } = [];

    public async Task OnGetAsync()
    {
        await Task.WhenAll([GetPhoto(), GetTodos()]);
    }

    public async Task OnPostAsync()
    {
        if (!string.IsNullOrWhiteSpace(Todo))
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage response = await httpClient.PostAsync("http://todoapp-svc:2345/todos", new StringContent(Todo));
            response.EnsureSuccessStatusCode();
        }

        await GetTodos();
    }

    private async Task GetTodos()
    {
        _todos.Clear();

        using HttpClient httpClient = _httpClientFactory.CreateClient();

        HttpResponseMessage response = await httpClient.GetAsync("http://todoapp-svc:2345/todos");
        response.EnsureSuccessStatusCode();

        string? jsonString = await response.Content.ReadAsStringAsync();
        List<string>? todos = JsonSerializer.Deserialize<List<string>>(jsonString);
        if (todos is not null)
        {
            _todos.AddRange(todos);
        }
    }

    private async Task GetPhoto()
    {
        // Don't get a new photo if one exists from a previous run
        if (Global.FirstTime && System.IO.File.Exists($"wwwroot/{PhotoFile}"))
        {
            _logger.LogInformation("Reusing existing photo from previous run");
            Global.NewPhoto = false;
        }

        Global.FirstTime = false;

        if (Global.NewPhoto)
        {
            _logger.LogInformation("Downloading a new photo");
            await DownloadImageAsync("https://picsum.photos/1200", $"wwwroot/{PhotoFile}");
            Global.NewPhoto = false;
        }
    }

    private async Task DownloadImageAsync(string imageUrl, string savePath)
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

