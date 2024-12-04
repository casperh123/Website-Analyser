using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public abstract class AbstractLinkExtractor<T> where T : Link
{
    protected readonly HtmlParser Parser;

    public AbstractLinkExtractor(HtmlParser parser)
    {
        Parser = parser;
    }

    public async Task<IEnumerable<T>> GetLinksFromDocument(HttpResponseMessage response, T referringUrl)
    {
        if (!response.IsSuccessStatusCode) return [];

        await using var document = await response.Content.ReadAsStreamAsync();

        return GetLinksFromDocument(Parser.ParseDocument(document), referringUrl);
    }

    protected abstract IEnumerable<T> GetLinksFromDocument(IDocument document, T referringUrl);

    protected static bool IsPage(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

        if (IsResourceFile(uri)) return false;

        return !IsExcluded(url);
    }

    protected static bool IsResourceFile(Uri uri)
    {
        HashSet<string> excludedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".js", ".css", ".png", ".jpg", ".jpeg", ".pdf", ".svg"
        };
        return excludedExtensions.Contains(Path.GetExtension(uri.LocalPath));
    }

    protected static bool IsExcluded(string url)
    {
        return url.Contains('#') || url.Contains('?') || url.Contains("mailto:");
    }
}