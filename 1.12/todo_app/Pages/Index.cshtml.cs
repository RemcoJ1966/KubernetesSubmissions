using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace todo_app.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    [BindProperty]
    public string PhotoFile { get; init; }

    [BindProperty]
    public string Todo { get; set; } = "";

    private readonly string _serviceUrl;
    private readonly string _picsumUrl;

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        
        PhotoFile = _configuration.GetValue<string>("PHOTO_FILE") ?? String.Empty;

        _serviceUrl = _configuration.GetValue<string>("SERVICE_URL") ?? String.Empty;
        _picsumUrl = _configuration.GetValue<string>("PICSUM_URL") ?? String.Empty;
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

            HttpResponseMessage response = await httpClient.PostAsync(_serviceUrl, new StringContent(Todo));
            response.EnsureSuccessStatusCode();
        }

        await GetTodos();
    }

    private async Task GetTodos()
    {
        _todos.Clear();

        using HttpClient httpClient = _httpClientFactory.CreateClient();

        HttpResponseMessage response = await httpClient.GetAsync(_serviceUrl);
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
            await DownloadImageAsync(_picsumUrl, $"wwwroot/{PhotoFile}");
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

