using System.Text;

string? port = Environment.GetEnvironmentVariable("PORT");

if (string.IsNullOrEmpty(port))
{
    Console.WriteLine("Error: PORT environment variable is not set.");
    Environment.Exit(-1); // Exit with error code -1
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();
app.Urls.Add($"http://0.0.0.0:{port}");

app.MapGet("/", () => Results.Content(FileContentsToHtml(), "text/html"));

Console.WriteLine($"Server started in port {port}");

app.Run();

string FileContentsToHtml()
{
    string path = @"/var/tmp/logoutput";

    StringBuilder strBuilder = new("<div>");

    using StreamReader reader = new(path);
    string? line;
    while ((line = reader.ReadLine()) is not null)
    {
        strBuilder.Append($"<p>{line}</p>");
    }

    strBuilder.Append("</div>");

    return strBuilder.ToString();
}