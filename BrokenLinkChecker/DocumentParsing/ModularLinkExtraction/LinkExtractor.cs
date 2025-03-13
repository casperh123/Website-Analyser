using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public class LinkExtractor : AbstractLinkExtractor<Link>
{
    // Cache the delegate for Uri.TryCreate to avoid repeated delegate creation
    private static readonly TryParseDelegate<Uri?> TryCreateUri = Uri.TryCreate;

    private delegate bool TryParseDelegate<T>(string uriString, UriKind uriKind, out T result);

    private const int InitialLinksCapacity = 256;

    // File extensions that should be excluded from crawling (not HTML pages)
    private static readonly HashSet<string> ExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Images
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp", ".ico", ".tiff",
        
        // Documents
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".ods", ".odp",
        
        // Archives
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".tgz",
        
        // Audio/Video
        ".mp3", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".wav", ".ogg", ".webm", ".m4a", ".m4v",
        
        // Other
        ".css", ".js", ".json", ".xml", ".rss", ".atom", ".csv", ".txt", ".rtf",
        ".exe", ".dll", ".apk", ".dmg", ".iso", ".bin", ".dat",
        
        // Fonts
        ".ttf", ".otf", ".woff", ".woff2", ".eot"
    };

    public LinkExtractor(HtmlParser parser) : base(parser)
    {
    }

    protected override IEnumerable<Link> GetLinksFromDocument(IDocument document, Link referringUrl)
    {
        string currentHost = new Uri(referringUrl.Target).Host;
        List<Link> links = new List<Link>(InitialLinksCapacity);

        ReadOnlySpan<char> hrefSpan;
        ReadOnlySpan<char> srcSpan;

        IHtmlCollection<IElement> elements = document.QuerySelectorAll("a[href], img[src]");

        foreach (IElement element in elements)
        {
            string? attributeValue;

            if (element is IHtmlAnchorElement anchor)
            {
                attributeValue = anchor.Href;
                hrefSpan = attributeValue.AsSpan();

                if (!hrefSpan.IsEmpty && !IsExcludedFast(hrefSpan) &&
                    IsValidHostMatch(attributeValue, currentHost) &&
                    !HasExcludedFileExtension(attributeValue))
                {
                    links.Add(new Link(attributeValue));
                }
            }
            else if (element is IHtmlImageElement img)
            {
                attributeValue = img.Source;
                srcSpan = attributeValue.AsSpan();

                if (!srcSpan.IsEmpty && !IsExcludedFast(srcSpan) &&
                    IsValidHostMatch(attributeValue, currentHost) &&
                    !HasExcludedFileExtension(attributeValue))
                {
                    links.Add(new Link(attributeValue));
                }
            }
        }

        return links;
    }

    private static bool IsExcludedFast(ReadOnlySpan<char> url)
    {
        // Exclude URLs with fragments or query parameters
        if (url.Contains('#') || url.Contains('?'))
        {
            return true;
        }

        // Exclude mailto: and tel: protocols
        if (url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    // Separate method for host matching to avoid repeated Uri creation
    private static bool IsValidHostMatch(string url, string currentHost)
    {
        if (TryCreateUri(url, UriKind.Absolute, out var uri))
        {
            // Get the host name from the URI
            string host = uri.Host;
        
            // Check if the host ends with .dk (Danish domain)
            return host.EndsWith(".dk", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
    
    // Check if the URL has a file extension that should be excluded
    private static bool HasExcludedFileExtension(string url)
    {
        // Try to parse as URI to handle complex URLs properly
        if (TryCreateUri(url, UriKind.Absolute, out var uri))
        {
            // Get the path part of the URI
            string path = uri.AbsolutePath;
            
            // Find the last dot in the path
            int lastDotIndex = path.LastIndexOf('.');
            
            // If there's a dot and it's in the last part of the path (not in a directory name)
            if (lastDotIndex > path.LastIndexOf('/'))
            {
                // Extract the extension including the dot
                string extension = path.Substring(lastDotIndex);
                
                // Check if it's in our excluded list
                return ExcludedExtensions.Contains(extension);
            }
        }
        
        // If we can't parse the URL or it doesn't have an extension, we assume it's not excluded
        return false;
    }
}