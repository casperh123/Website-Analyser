using System.Collections.Concurrent;
using System.Net;
using BrokenLinkChecker.DocumentParsing.Linkextraction;
using BrokenLinkChecker.models;
using BrokenLinkChecker.Networking;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.crawler
{
    public class Crawler
    {
        private readonly HttpClient _httpClient;
        private readonly HttpRequestHandler _requestHandler;
        private readonly LinkExtractor _linkExtractor;
        private readonly ConcurrentDictionary<string, HttpStatusCode> _visitedResources = new();
        
        private CrawlerConfig CrawlerConfig { get; }
        private CrawlResult CrawlResult { get; }

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig, CrawlResult crawlResult)
        { 
            _requestHandler = new HttpRequestHandler(httpClient, crawlerConfig);
            CrawlerConfig = crawlerConfig ?? throw new ArgumentNullException(nameof(crawlerConfig));
            CrawlResult = crawlResult ?? throw new ArgumentNullException(nameof(crawlResult));
            _linkExtractor = new LinkExtractor(CrawlerConfig);
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

                (HttpResponseMessage response, long requestTime) = await _requestHandler.RequestPageWithBenchmarkAsync(url);

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
            if(!response.IsSuccessStatusCode)
            {
                CrawlResult.HandleBrokenLink(url, response);
                return [];
            }
    
            (List<Link> links, long parseTime) = await Utilities.BenchmarkAsync(() => _linkExtractor.GetLinksFromResponseAsync(response, url));

            PageStat pageStat = new PageStat(url.Target, response, url.Type, requestTime, parseTime);
            CrawlResult.AddResource(pageStat);

            return links.Where(link => !Utilities.IsAsyncOrFragmentRequest(link.Target));
        }
    }
}