using System.Collections.Concurrent;
using System.Net;
using BrokenLinkChecker.DocumentParsing.Linkextraction;
using BrokenLinkChecker.models;
using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.Networking;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.crawler;

public class Crawler
{
    private readonly DocumentParsing.Linkextraction.LinkExtractor _linkProcessor;
    private readonly HttpRequestHandler _requestHandler;
    private readonly ConcurrentDictionary<string, HttpStatusCode> _visitedResources = new();

    public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig, CrawlResult crawlResult)
    {
        _requestHandler = new HttpRequestHandler(httpClient, crawlerConfig);
        CrawlerConfig = crawlerConfig;
        CrawlResult = crawlResult;
        _linkProcessor = new DocumentParsing.Linkextraction.LinkExtractor(CrawlerConfig);
    }

    private CrawlerConfig CrawlerConfig { get; }
    private CrawlResult CrawlResult { get; }

    public async Task CrawlWebsiteAsync(Uri url)
    {
        ConcurrentQueue<TraceableLink> linkQueue = new();

        linkQueue.Enqueue(new TraceableLink(string.Empty, url.ToString(), string.Empty, 0, ResourceType.Page));

        while (!linkQueue.IsEmpty)
        {
            ConcurrentQueue<TraceableLink> foundLinks = new();

            await Parallel.ForEachAsync(linkQueue,
                async (link, cancellationToken) => { await ProcessLinkAsync(link, foundLinks); });

            CrawlResult.SetLinksEnqueued(foundLinks.Count);

            linkQueue = foundLinks;
        }
    }

    private async Task ProcessLinkAsync(TraceableLink url, ConcurrentQueue<TraceableLink> linksFound)
    {
        if (_visitedResources.TryAdd(url.Target, HttpStatusCode.Unused))
        {
            IEnumerable<TraceableLink> links = await RequestAndProcessPage(url);
            foreach (var link in links) linksFound.Enqueue(link);
            CrawlResult.IncrementLinksChecked();
        }
        else
        {
            CrawlResult.AddResource(url, _visitedResources[url.Target]);
        }
    }

    private async Task<IEnumerable<TraceableLink>> RequestAndProcessPage(TraceableLink url)
    {
        await CrawlerConfig.Semaphore.WaitAsync();
        try
        {
            var (response, requestTime) = await Utilities.BenchmarkAsync(() => _requestHandler.RequestPageAsync(url));
            _visitedResources[url.Target] = response.StatusCode;

            (IEnumerable<TraceableLink> links, var parseTime) =
                await Utilities.BenchmarkAsync(() => _linkProcessor.GetLinksFromResponseAsync(response, url));
            CrawlResult.AddResource(url, response, requestTime, parseTime);

            return links;
        }
        finally
        {
            CrawlerConfig.Semaphore.Release();
        }
    }
}