using AngleSharp;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.LinkProcessors;

public class LinkProcessor : ILinkProcessor<Link>
{
    private readonly HttpClient _httpClient;
    private readonly AbstractLinkExtractor<Link> _linkExtractor;
    private HashSet<string> _visitedPages;
    private HashSet<string> _enqueuedPages;

    public LinkProcessor(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        _linkExtractor = new StreamLinkExtractor(new HtmlParser());
        _visitedPages = new HashSet<string>();
        _enqueuedPages = new HashSet<string>();
    }

    public void FlushCache()
    {
        _visitedPages = new HashSet<string>();
        _enqueuedPages = new HashSet<string>();
    }

    public async Task<IEnumerable<Link>> ProcessLinkAsync(Link link)
    {
        IEnumerable<Link> links = Array.Empty<Link>();

        if (!_visitedPages.Add(link.Target))
        {
            return links;
        }

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                link.Target,
                HttpCompletionOption.ResponseHeadersRead
            ).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                await using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                links = await _linkExtractor.GetLinksFromStream(responseStream, link).ConfigureAwait(false);
                links = links.Where(l => _enqueuedPages.Add(l.Target));
            }
        }
        catch (HttpRequestException)
        {
            links = Array.Empty<Link>();
        }

        return links;
    }
}