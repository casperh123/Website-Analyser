using AngleSharp.Html.Parser;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.LinkProcessors;

public class LinkProcessor : ILinkProcessor<Link>
{
    private readonly HttpClient _httpClient;
    private readonly HashSet<string> _visitedSites;
    private AbstractLinkExtractor<Link> _linkExtractor;

    public LinkProcessor(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _visitedSites = [];
        _linkExtractor = new LinkExtractor(new HtmlParser());
    }

    public async Task<IEnumerable<Link>> ProcessLinkAsync(Link link, ModularCrawlResult<Link> crawlResult)
    {
        IEnumerable<Link> links = [];

        try
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync(link.Target, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                links = await _linkExtractor.GetLinksFromDocument(response, link);
            }
        }
        catch (HttpRequestException e)
        {
            links = [];
        }
        finally
        {
            _visitedSites.Add(link.Target);
        }
        
        
        return links;
    }
}