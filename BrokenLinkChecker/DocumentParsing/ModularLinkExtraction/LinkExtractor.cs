using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public class LinkExtractor(HtmlParser parser) : AbstractLinkExtractor<Link>(parser)
{
    protected override IEnumerable<Link> GetLinksFromDocument(IDocument document, Link referringUrl)
    {
        List<Link> links = new();
        var currentUrl = new Uri(referringUrl.Target);

        foreach (var link in document.Links)
        {
            var href = link.GetAttribute("href");
            if (!string.IsNullOrEmpty(href) && !IsExcluded(href)) links.Add(new Link(href));
        }

        foreach (var image in document.Images)
        {
            var src = image.Source;
            if (!string.IsNullOrEmpty(src) && !IsExcluded(src)) links.Add(new Link(src));
        }

        return links.Where(link =>
            Uri.TryCreate(link.Target, UriKind.Absolute, out var uri) && uri.Host == currentUrl.Host).ToList();
    }
}