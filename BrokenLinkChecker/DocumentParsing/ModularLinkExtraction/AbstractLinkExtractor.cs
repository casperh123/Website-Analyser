using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public abstract class AbstractLinkExtractor<T> where T : TargetLink
{
    protected readonly HtmlParser Parser;

    public AbstractLinkExtractor(HtmlParser parser)
    {
        Parser = parser;
    }

    public async Task<IEnumerable<T>> GetLinksFromDocument(HttpResponseMessage response, T referringUrl)
    {
        if (!response.IsSuccessStatusCode)
        {
            return Enumerable.Empty<T>();
        } 

        await using Stream document = await response.Content.ReadAsStreamAsync();
        
        return GetLinksFromDocument(Parser.ParseDocument(document), referringUrl);
    }
    
    protected abstract IEnumerable<T> GetLinksFromDocument(IDocument document, T referringUrl);
    
    protected static bool IsPage(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        {
            return false;
        }

        if (IsResourceFile(uri))
        {
            return false;
        }

        return !ContainsExcludableElements(url);
    }

    protected static bool IsResourceFile(Uri uri)
    {
        HashSet<string> excludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
        {
            ".js", ".css", ".png", ".jpg", ".jpeg", ".pdf", ".svg"
        };
        return excludedExtensions.Contains(Path.GetExtension(uri.LocalPath));
    }

    protected static bool ContainsExcludableElements(string url)
    {
        return url.Contains('#') || url.Contains('?') || url.Contains("mailto:");
    }
}