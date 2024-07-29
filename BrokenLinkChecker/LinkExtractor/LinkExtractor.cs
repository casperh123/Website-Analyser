using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;

public class LinkExtractor
{
    private readonly IConfiguration _config;

    public LinkExtractor(IConfiguration config)
    {
        _config = config;
    }

    public async Task<List<LinkNode>> GetLinksFromResponseAsync(HttpResponseMessage response, LinkNode url)
    {
        string? contentType = response.Content.Headers.ContentType?.MediaType;
        
        if (contentType != null && contentType.Equals("text/html", StringComparison.OrdinalIgnoreCase))
        {
            await using Stream document = await response.Content.ReadAsStreamAsync();
            return await ExtractLinksFromDocumentAsync(document, url);
        }

        return new List<LinkNode>();
    }
    
    private async Task<List<LinkNode>> ExtractLinksFromDocumentAsync(Stream document, LinkNode checkingUrl)
    {
        List<LinkNode> links = new List<LinkNode>();
        IBrowsingContext context = BrowsingContext.New(_config);
        IHtmlParser parser = context.GetService<IHtmlParser>() ?? new HtmlParser();

        IDocument doc = await parser.ParseDocumentAsync(document);
        IHtmlCollection<IElement> documentLinks = doc.QuerySelectorAll("a[href]");

        foreach (IElement link in documentLinks)
        {
            string href = link.GetAttribute("href");
            if (!string.IsNullOrEmpty(href))
            {
                LinkNode newLink = GenerateLinkNode(link, checkingUrl.Target);
                if (Uri.TryCreate(newLink.Target, UriKind.Absolute, out Uri uri) && uri.Host == new Uri(checkingUrl.Target).Host)
                {
                    links.Add(newLink);
                }
            }
        }

        return links;
    }
    
    private LinkNode GenerateLinkNode(IElement link, string target)
    {
        string href = link.GetAttribute("href") ?? string.Empty;
        string resolvedUrl;
        try
        {
            resolvedUrl = Utilities.GetUrl(target, href);
        }
        catch (UriFormatException)
        {
            resolvedUrl = href; // Fall back to original href or handle as needed
        }
        string text = link.TextContent;
        int line = link.SourceReference?.Position.Line ?? -1;

        return new LinkNode(target, resolvedUrl, text, line);
    }
}
