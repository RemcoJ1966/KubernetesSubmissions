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

app.MapGet("/", () => Results.Content(ComposeHtml(), "text/html"));

Console.WriteLine($"Server started in port {port}");

app.Run();

string ComposeHtml()
{
    StringBuilder strBuilder = new("<div>");

    strBuilder = FileContentsToHtml(strBuilder, @"/var/tmp/logoutput");
    strBuilder = FileContentsToHtml(strBuilder, @"/var/tmp/pingpong");

    strBuilder.Append("</div>");

    return strBuilder.ToString();
}

StringBuilder FileContentsToHtml(StringBuilder strBuilder, string path)
{
    if (File.Exists(path))
    {
        using StreamReader reader = new(path);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            strBuilder.Append($"<p>{line}</p>");
        }
    }

    return strBuilder;
}
