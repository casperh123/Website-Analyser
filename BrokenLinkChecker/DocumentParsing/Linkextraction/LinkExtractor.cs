using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.crawler;
using BrokenLinkChecker.DocumentParsing.Browsing;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.DocumentParsing.Linkextraction;

public class LinkExtractor
{
    private HtmlParserPool _htmlParserPool;

    public LinkExtractor(CrawlerConfig crawlerConfig)
    {
        _htmlParserPool = new HtmlParserPool(
            new HtmlParserOptions
            {
                IsKeepingSourceReferences = true
            }, 
            crawlerConfig.ConcurrentRequests);
    }
    
    public async Task<List<LinkNode>> GetLinksFromResponseAsync(HttpResponseMessage response, LinkNode url)
    {
        if (!IsHtmlContent(response))
        {
            return [];
        }

        await using Stream document = await response.Content.ReadAsStreamAsync();
        return await ExtractLinksFromDocumentAsync(document, url);
    }

    private bool IsHtmlContent(HttpResponseMessage response)
    {
        string? contentType = response.Content.Headers.ContentType?.MediaType;
        return contentType != null && contentType.Equals("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<List<LinkNode>> ExtractLinksFromDocumentAsync(Stream document, LinkNode checkingUrl)
    {
        List<LinkNode> links = new List<LinkNode>();
        IDocument doc;
        Uri thisUrl = new Uri(checkingUrl.Target);

        
        using (PooledHtmlParser pooledHtmlParser = await _htmlParserPool.GetParserAsync())
        {
            doc = await pooledHtmlParser.Parser.ParseDocumentAsync(document);
        }
        
        
        foreach (IElement link in doc.QuerySelectorAll("a[href]"))
        {
            LinkNode newLink = GenerateLinkNode(link, checkingUrl.Target);
            if (Uri.TryCreate(newLink.Target, UriKind.Absolute, out Uri uri) && uri.Host == thisUrl.Host)
            {
                links.Add(newLink);
            }
        }
        
        return links;
    }

    private LinkNode GenerateLinkNode(IElement link, string target)
    {
        string href = link.GetAttribute("href") ?? string.Empty;
        string resolvedUrl = Utilities.GetUrl(target, href);  // Utilities.GetUrl should handle exceptions and return href as fallback
        string text = link.TextContent;
        int line = link.SourceReference?.Position.Line ?? -1;

        return new LinkNode(target, resolvedUrl, text, line);
    }
}