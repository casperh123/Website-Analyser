using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public class TargetLinkExtractor() : AbstractLinkExtractor<TargetLink>(new HtmlParser(new HtmlParserOptions()))
{
    protected override IEnumerable<TargetLink> GetLinksFromDocument(IDocument document, TargetLink referringUrl)
    {
        List<TargetLink> links = [];
        Uri thisUrl = new Uri(referringUrl.Target);
            
        foreach (IElement link in document.Links)
        {
            string? href = link.GetAttribute("href");
            if (!string.IsNullOrEmpty(href) && IsPage(href))
            {
                TargetLink newLink = new (href);
                links.Add(newLink);
            }
        }

        return links.Where(link => Uri.TryCreate(link.Target, UriKind.Absolute, out Uri uri) && uri.Host == thisUrl.Host).ToList();
    }
}