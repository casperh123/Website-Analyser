using System.Collections.Concurrent;
using System.Net;
using BrokenLinkChecker.DocumentParsing.Linkextraction;
using BrokenLinkChecker.models;
using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.Networking;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.crawler
{
    public class Crawler
    {
        private readonly HttpRequestHandler _requestHandler;
        private readonly LinkExtractor _linkProcessor;
        private readonly ConcurrentDictionary<string, HttpStatusCode> _visitedResources = new();
        
        private CrawlerConfig CrawlerConfig { get; }
        private CrawlResult CrawlResult { get; }

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig, CrawlResult crawlResult)
        { 
            _requestHandler = new HttpRequestHandler(httpClient, crawlerConfig);
            CrawlerConfig = crawlerConfig;
            CrawlResult = crawlResult;
            _linkProcessor = new LinkExtractor(CrawlerConfig);
        }

        public async Task CrawlWebsiteAsync(Uri url)
        {
            ConcurrentQueue<TraceableLink> linkQueue = new ConcurrentQueue<TraceableLink>();

            linkQueue.Enqueue(new TraceableLink(string.Empty, url.ToString(), string.Empty, 0, ResourceType.Page));

            while (!linkQueue.IsEmpty)
            {
                ConcurrentQueue<TraceableLink> foundLinks = new ConcurrentQueue<TraceableLink>();

                await Parallel.ForEachAsync(linkQueue, async (link, cancellationToken) =>
                {
                    await ProcessLinkAsync(link, foundLinks);
                });

                CrawlResult.SetLinksEnqueued(foundLinks.Count);

                linkQueue = foundLinks;
            }
        }

        private async Task ProcessLinkAsync(TraceableLink url, ConcurrentQueue<TraceableLink> linksFound)
        {
            if (_visitedResources.TryAdd(url.Target, HttpStatusCode.Unused))
            {
                IEnumerable<TraceableLink> links = await RequestAndProcessPage(url);
                foreach (TraceableLink link in links)
                {
                    linksFound.Enqueue(link);
                }
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
                (HttpResponseMessage response, long requestTime) = await Utilities.BenchmarkAsync(() => _requestHandler.RequestPageAsync(url));
                _visitedResources[url.Target] = response.StatusCode;

                (IEnumerable<TraceableLink> links, long parseTime) = await Utilities.BenchmarkAsync(() => _linkProcessor.GetLinksFromResponseAsync(response, url));
                CrawlResult.AddResource(url, response, requestTime, parseTime);

                return links;
            }
            finally
            {
                CrawlerConfig.Semaphore.Release();
            }
        }
    }
}