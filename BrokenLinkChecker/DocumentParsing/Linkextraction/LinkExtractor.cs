using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.crawler;
using BrokenLinkChecker.Crawler.BaseCrawler;
using BrokenLinkChecker.DocumentParsing.Browsing;
using BrokenLinkChecker.models;
using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.DocumentParsing.Linkextraction;

public class LinkExtractor
{
    private readonly CrawlerConfig _crawlerConfig;
    private readonly HtmlParserPool _htmlParserPool;

    public LinkExtractor(CrawlerConfig crawlerConfig)
    {
        _crawlerConfig = crawlerConfig;
        var htmlParsingOptions = new HtmlParserOptions { IsKeepingSourceReferences = true };
        _htmlParserPool = new HtmlParserPool(htmlParsingOptions, crawlerConfig.ConcurrentRequests);
    }

    public async Task<IEnumerable<TraceableLink>> GetLinksFromResponseAsync(HttpResponseMessage response,
        TraceableLink url)
    {
        if (!response.IsSuccessStatusCode || url.Type is not ResourceType.Page)
        {
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            return Enumerable.Empty<TraceableLink>();
        }

        await using var document = await response.Content.ReadAsStreamAsync();
        return await ExtractLinksFromDocumentAsync(document, url);
    }

    private async Task<List<TraceableLink>> ExtractLinksFromDocumentAsync(Stream document, TraceableLink checkingUrl)
    {
        IDocument doc;

        using (var pooledHtmlParser = await _htmlParserPool.GetParserAsync())
        {
            doc = await pooledHtmlParser.Parser.ParseDocumentAsync(document);
        }

        return GetLinksFromDocument(doc, checkingUrl);
    }

    private List<TraceableLink> GetLinksFromDocument(IDocument document, TraceableLink checkingUrl)
    {
        List<TraceableLink> links = [];
        var thisUrl = new Uri(checkingUrl.Target);

        foreach (var link in document.Links)
        {
            var href = link.GetAttribute("href");
            if (!string.IsNullOrEmpty(href) && !IsExcluded(href))
            {
                var newTraceableLink = GenerateLinkNode(link, checkingUrl.Target, "href", ResourceType.Page);
                links.Add(newTraceableLink);
            }
        }

        if (_crawlerConfig.CrawlMode is CrawlMode.CacheWarm)
            return links.Where(link =>
                Uri.TryCreate(link.Target, UriKind.Absolute, out var uri) && uri.Host == thisUrl.Host).ToList();

        foreach (var stylesheet in document.StyleSheets)
        {
            var href = stylesheet.Href;
            if (!string.IsNullOrEmpty(href))
            {
                var newTraceableLink = GenerateLinkNode(stylesheet.OwnerNode, checkingUrl.Target, "href",
                    ResourceType.Stylesheet);
                links.Add(newTraceableLink);
            }
        }

        foreach (var image in document.Images)
        {
            var src = image.Source;
            if (!string.IsNullOrEmpty(src))
            {
                var newTraceableLink = GenerateLinkNode(image, checkingUrl.Target, "src", ResourceType.Image);
                links.Add(newTraceableLink);
            }
        }

        return links
            .Where(link => Uri.TryCreate(link.Target, UriKind.Absolute, out var uri) && uri.Host == thisUrl.Host)
            .ToList();
    }

    private TraceableLink GenerateLinkNode(IElement element, string target, string attribute,
        ResourceType resourceType = ResourceType.Resource)
    {
        var href = element.GetAttribute(attribute) ?? string.Empty;
        var resolvedUrl = Utilities.GetUrl(target, href);
        var text = element.TextContent;
        var line = element.SourceReference?.Position.Line ?? -1;

        return new TraceableLink(target, resolvedUrl, text, line, resourceType);
    }

    private static bool IsExcluded(string url)
    {
        HashSet<string> excludedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".js", ".css", "" };
        HashSet<string> asyncKeywords = new(StringComparer.OrdinalIgnoreCase) { "ajax", "async", "action=async" };

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            if (excludedExtensions.Contains(Path.GetExtension(uri.LocalPath)))
                return false;

        return asyncKeywords.Any(url.Contains) || url.Contains('#') || url.Contains('?') || url.Contains("mailto");
    }
}