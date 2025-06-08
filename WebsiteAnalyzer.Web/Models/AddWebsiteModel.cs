namespace WebsiteAnalyzer.Web.Models;

public class AddWebsiteModel
{
    public string Name { get; }
    public string Url { get; }

    public AddWebsiteModel(string name, string url)
    {
        Name = name;
        Url = url;
    }
}