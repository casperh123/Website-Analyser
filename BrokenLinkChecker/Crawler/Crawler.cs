using System.Collections.Concurrent;
using System.Net;
using BrokenLinkChecker.models;
using BrokenLinkChecker.Networking;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.crawler
{
    public class Crawler
    {
        private readonly HttpRequestHandler _requestHandler;
        private readonly LinkProcessor _linkProcessor;
        private readonly ConcurrentDictionary<string, HttpStatusCode> _visitedResources = new();
        
        private CrawlerConfig CrawlerConfig { get; }
        private CrawlResult CrawlResult { get; }

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig, CrawlResult crawlResult)
        { 
            _requestHandler = new HttpRequestHandler(httpClient, crawlerConfig);
            CrawlerConfig = crawlerConfig;
            CrawlResult = crawlResult;
            _linkProcessor = new LinkProcessor(CrawlerConfig);
        }

        public async Task<List<PageStat>> CrawlWebsiteAsync(Uri url)
        {
            ConcurrentQueue<Link> linkQueue = new ConcurrentQueue<Link>();

            linkQueue.Enqueue(new Link(string.Empty, url.ToString(), string.Empty, 0, ResourceType.Page));

            do
            {
                OrderablePartitioner<Link> partitioner = Partitioner.Create(linkQueue);
                ConcurrentQueue<Link> foundLinks = new ConcurrentQueue<Link>();

                await Parallel.ForEachAsync(partitioner.GetDynamicPartitions(),
                    async (link, cancellationToken) =>
                    {
                        await ProcessLinkAsync(link, foundLinks);
                    });

                CrawlResult.SetLinksEnqueued(foundLinks.Count);
                linkQueue = foundLinks;

            } while (linkQueue.Count > 0);

            return CrawlResult.VisitedPages.ToList();
        }

        private async Task ProcessLinkAsync(Link url, ConcurrentQueue<Link> linksFound)
        {
            if (!_visitedResources.TryAdd(url.Target, HttpStatusCode.Unused))
            {
                if (_visitedResources[url.Target] is HttpStatusCode status && status != HttpStatusCode.OK)
                {
                    CrawlResult.HandleBrokenLink(url, status);
                }
                return;
            }

            IEnumerable<Link> links = await RequestAndProcessPage(url);

            foreach (Link link in links)
            {
                linksFound.Enqueue(link);
            }
            
            CrawlResult.IncrementLinksChecked();
        }

        private async Task<IEnumerable<Link>> RequestAndProcessPage(Link url)
        {
            try
            {
                await CrawlerConfig.Semaphore.WaitAsync();

                (HttpResponseMessage response, long requestTime) = await Utilities.BenchmarkAsync(() => _requestHandler.RequestPageAsync(url));
                
                _visitedResources[url.Target] = response.StatusCode;

                return await ProcessResponse(response, url, requestTime);
            }
            finally
            {
                CrawlerConfig.Semaphore.Release();
            }
        }
        
        private async Task<IEnumerable<Link>> ProcessResponse(HttpResponseMessage response, Link url, long requestTime)
        {
            (IEnumerable<Link> links, long parseTime) = await Utilities.BenchmarkAsync(() => _linkProcessor.GetLinksFromResponse(response, url));

            PageStat pageStat = new PageStat(url.Target, response, url.Type, requestTime, parseTime);
            CrawlResult.AddResource(pageStat);

            return links;
        }
    }
}