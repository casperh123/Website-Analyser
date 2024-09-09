using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public abstract class AbstractLinkExtrator<T> where T : NavigationLink
{
    protected readonly HtmlParser Parser;

    public AbstractLinkExtrator(HtmlParser parser)
    {
        Parser = parser;
    }

    public async Task<IEnumerable<T>> GetLinksFromResponseAsync(HttpResponseMessage response, T link)
    {
        if (!response.IsSuccessStatusCode)
        {
            return Enumerable.Empty<T>();
        } 

        await using Stream document = await response.Content.ReadAsStreamAsync();
        
        return ExtractLinksFromDocument(Parser.ParseDocument(document), link);
    }
    
    protected abstract List<T> ExtractLinksFromDocument(IDocument document, T link);
}