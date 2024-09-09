using System.Net;
using AngleSharp.Dom;
using BrokenLinkChecker.crawler;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;
using BrokenLinkChecker.models.Links;
using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.DocumentParsing.LinkProcessors;

public class BrokenLinkProcessor : ILinkProcessor<IndexedLink>
{
    private readonly Dictionary<string, HttpStatusCode> _visitedResources = new();
    private readonly HttpClient _httpClient;
    private AbstractLinkExtrator<IndexedLink> _linkExtrator; 
    
    public BrokenLinkProcessor(HttpClient httpClient, AbstractLinkExtrator<IndexedLink> linkExtrator)
    {
        _httpClient = httpClient;
        _linkExtrator = linkExtrator;
    } 
    
    public async Task<IEnumerable<IndexedLink>> ProcessLinkAsync(IndexedLink link, ModularCrawlResult<IndexedLink> crawlResult)
    {
        IEnumerable<IndexedLink> links = [];
        
        if (_visitedResources.TryGetValue(link.Target, out HttpStatusCode statusCode))
        {
            if (statusCode != HttpStatusCode.OK)
            {
                
            }
        }
        else
        {
            links = await RequestAndProcessPage(link.Target);
        }
            
        crawlResult.IncrementLinksChecked();
        crawlResult.AddResource(url, _visitedResources[url.Target]);

        return links;
    }

    private async Task<IEnumerable<IndexedLink>> RequestAndProcessPage(string url)
    {
        (HttpResponseMessage response, long requestTime) =
            await Utilities.BenchmarkAsync(() => _httpClient.GetAsync(url));
        _visitedResources[url] = response.StatusCode;

        (IEnumerable<Index> links, long parseTime) =
            await Utilities.BenchmarkAsync(() => _linkExtrator.GetLinksFromResponseAsync(response, url));

        CrawlResult.AddResource();

        return links;
    }
}