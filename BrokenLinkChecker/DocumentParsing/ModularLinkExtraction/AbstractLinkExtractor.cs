using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public abstract class AbstractLinkExtractor<T> where T : NavigationLink
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
    
    protected bool IsPage(string url)
    {
        // Try to create a Uri object from the URL string.
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        {
            return false;  // Not a valid absolute URL
        }

        // Check if the URI's path ends with a common file extension for resources.
        if (IsResourceFile(uri))
        {
            return false;  // It's a resource file, not a web page
        }

        // Check for URL elements that typically do not correspond to web pages.
        return !ContainsExcludableElements(url);
    }

    protected bool IsResourceFile(Uri uri)
    {
        HashSet<string> excludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
        {
            ".js", ".css", ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".svg", ".webp", ".mp4", ".mp3"
        };

        return excludedExtensions.Contains(Path.GetExtension(uri.AbsolutePath));
    }

    protected bool ContainsExcludableElements(string url)
    {
        string[] partsToExclude = { "#", "?", "mailto:", "ftp:", "javascript:" };

        return partsToExclude.Any(part => url.Contains(part, StringComparison.OrdinalIgnoreCase));
    }
    
    protected static bool IsSameDomain(Uri rootDomainUri, string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        {
            return uri.GetLeftPart(UriPartial.Authority).Equals(rootDomainUri.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}