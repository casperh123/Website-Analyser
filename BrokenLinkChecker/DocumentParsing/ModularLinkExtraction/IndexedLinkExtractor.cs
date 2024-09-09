using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public class IndexedLinkExtractor(HtmlParser parser) : AbstractLinkExtractor<IndexedLink>(parser)
{
    protected override IEnumerable<IndexedLink> GetLinksFromDocument(IDocument document, IndexedLink referringUrl)
    {
        List<IndexedLink> links = [];
        Uri thisUrl = new Uri(referringUrl.Target);
            
        foreach (IElement link in document.Links)
        {
            links.Add(GenerateIndexedLink(link, referringUrl.Target));
        }

        return links.Where(link => Uri.TryCreate(link.Target, UriKind.Absolute, out Uri uri) && uri.Host == thisUrl.Host).ToList();
    }
    
    private IndexedLink GenerateIndexedLink(IElement element, string target)
    {
        string href = element.GetAttribute("href") ?? string.Empty;
        string resolvedUrl = Utilities.GetUrl(target, href);
        string text = element.TextContent;
        int line = element.SourceReference?.Position.Line ?? -1;

        return new IndexedLink(target, resolvedUrl, text, line);
    }
}