using AngleSharp.Html.Parser;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.LinkProcessors;

public class LinkProcessor : ILinkProcessor<Link>
{
    private readonly HttpClient _httpClient;
    private readonly AbstractLinkExtractor<Link> _linkExtractor;
    private HashSet<string> _visitedPages;    // Tracks processed pages
    private HashSet<string> _enqueuedPages;   // Tracks enqueued pages

    public LinkProcessor(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _linkExtractor = new LinkExtractor(new HtmlParser());
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
        // If the link has already been visited, return an empty result
        if (!_visitedPages.Add(link.Target))
        {
            return Array.Empty<Link>();
        }

        IEnumerable<Link> links = Array.Empty<Link>();

        try
        {
            // Fetch the page
            HttpResponseMessage response = await _httpClient.GetAsync(
                link.Target,
                HttpCompletionOption.ResponseHeadersRead
            ).ConfigureAwait(false);

            // If the response is successful, extract links
            if (response.IsSuccessStatusCode)
            {
                links = await _linkExtractor.GetLinksFromDocument(response, link).ConfigureAwait(false);

                // Filter out links that are already enqueued
                links = links.Where(l => _enqueuedPages.Add(l.Target)).ToList();
            }
        }
        catch (HttpRequestException)
        {
            // Handle request failures gracefully by returning an empty list
            links = Array.Empty<Link>();
        }

        return links;
    }
}