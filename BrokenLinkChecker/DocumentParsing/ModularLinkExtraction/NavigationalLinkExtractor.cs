using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public class NavigationLinkExtractor() : AbstractLinkExtrator<NavigationLink>(new HtmlParser(new HtmlParserOptions()))
{
    protected override List<NavigationLink> ExtractLinksFromDocument(IDocument document, NavigationLink link)
    {
        return GetLinksFromDocument(document, link);
    }

    private List<NavigationLink> GetLinksFromDocument(IDocument document, NavigationLink checkingUrl)
    {
        List<NavigationLink> links = [];
        Uri thisUrl = new Uri(checkingUrl.Target);
            
        foreach (IElement link in document.Links)
        {
            string? href = link.GetAttribute("href");
            if (!string.IsNullOrEmpty(href) && !IsExcluded(href))
            {
                NavigationLink newLink = new (href);
                links.Add(newLink);
            }
        }

        return links.Where(link => Uri.TryCreate(link.Target, UriKind.Absolute, out Uri uri) && uri.Host == thisUrl.Host).ToList();
    }
    
    private static bool IsExcluded(string url)
    {
        HashSet<string> excludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".js", ".css" };
        HashSet<string> asyncKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ajax", "async", "action=async" };

        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        {
            if (excludedExtensions.Contains(Path.GetExtension(uri.LocalPath)))
            {
                return false;
            }
        }

        return asyncKeywords.Any(url.Contains) || url.Contains('#') || url.Contains('?') || url.Contains("mailto");
    }
}