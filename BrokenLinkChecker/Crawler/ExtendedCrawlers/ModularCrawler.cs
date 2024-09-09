using System.Collections.Concurrent;
using System.Net;
using BrokenLinkChecker.crawler;
using BrokenLinkChecker.DocumentParsing.Linkextraction;
using BrokenLinkChecker.models;
using BrokenLinkChecker.models.Links;
using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.Networking;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawler<T> where T : NavigationLink
{
    private readonly HttpRequestHandler _requestHandler;
    private readonly LinkExtractor _linkProcessor;
    private readonly ConcurrentDictionary<string, HttpStatusCode> _visitedResources = new();
    private CrawlerConfig CrawlerConfig { get; }
    private ModularCrawlResult<T> CrawlResult { get; }
    private Queue<NavigationLink> LinkQueue { get; set; }

    public ModularCrawler(HttpClient httpClient, CrawlerConfig crawlerConfig, ModularCrawlResult<T> crawlResult)
    { 
        _requestHandler = new HttpRequestHandler(httpClient, crawlerConfig);
        CrawlerConfig = crawlerConfig;
        CrawlResult = crawlResult;
        _linkProcessor = new LinkExtractor(CrawlerConfig);
    }

    public async Task CrawlWebsiteAsync(Uri url)
    {
        LinkQueue = [];
        LinkQueue.Enqueue(new NavigationLink(url.ToString()));

        while (!LinkQueue.TryDequeue(out NavigationLink link))
        {
            await ProcessLinkAsync(link);

            CrawlResult.SetLinksEnqueued(LinkQueue.Count);
        }
    }

    private async Task ProcessLinkAsync(NavigationLink url)
    {
        if (_visitedResources.TryAdd(url.Target, HttpStatusCode.Unused))
        {
            IEnumerable<Link> links = await RequestAndProcessPage(url);
            foreach (Link link in links)
            {
                LinkQueue.Enqueue(link);
            }
            CrawlResult.IncrementLinksChecked();
        }
        else
        {
            CrawlResult.AddResource(url, _visitedResources[url.Target]);
        }
    }

    private async Task<IEnumerable<Link>> RequestAndProcessPage(NavigationLink url)
    {
 
        (HttpResponseMessage response, long requestTime) = await Utilities.BenchmarkAsync(() => _requestHandler.RequestPageAsync(url));
        _visitedResources[url.Target] = response.StatusCode;

        (IEnumerable<Link> links, long parseTime) = await Utilities.BenchmarkAsync(() => _linkProcessor.GetLinksFromResponseAsync(response, url));
        CrawlResult.AddResource(url, response, requestTime, parseTime);

        return links;
    }
}