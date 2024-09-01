using System.Collections.Concurrent;
using System.Net;
using BrokenLinkChecker.DocumentParsing.Linkextraction;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.crawler
{
    public class Crawler
    {
        private readonly HttpClient _httpClient;
        private readonly LinkExtractor _linkExtractor;
        private readonly ConcurrentDictionary<string, PageStats> _visitedResources = new();
        
        private CrawlerConfig CrawlerConfig { get; }
        private CrawlResult CrawlResult { get; }

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig, CrawlResult crawlResult)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            CrawlerConfig = crawlerConfig ?? throw new ArgumentNullException(nameof(crawlerConfig));
            _linkExtractor = new LinkExtractor(CrawlerConfig);
            CrawlResult = crawlResult ?? throw new ArgumentNullException(nameof(crawlResult));
        }

        public async Task<List<PageStats>> CrawlWebsiteAsync(Uri url)
        {
            var linkQueue = new ConcurrentBag<Link> { new Link(string.Empty, url.ToString(), string.Empty, 0) };

            do
            {
                var partitioner = Partitioner.Create(linkQueue);
                var foundLinks = new ConcurrentBag<Link>();

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

        private async Task ProcessLinkAsync(Link url, ConcurrentBag<Link> linksFound)
        {
            var pageStats = new PageStats(url.Target, HttpStatusCode.Unused);
            
            if (!_visitedResources.TryAdd(url.Target, pageStats))
            {
                pageStats = _visitedResources[url.Target];
                
                if (pageStats.StatusCode is HttpStatusCode.OK or HttpStatusCode.Unused)
                {
                    return;
                }

                CrawlResult.AddBrokenLink(new BrokenLink(url, pageStats.StatusCode));
                return;
            }

            var links = await GetLinksFromPage(url, pageStats);

            foreach (var link in links.Where(link => !Utilities.IsAsyncOrFragmentRequest(link.Target)))
            {
                linksFound.Add(link);
            }

            CrawlResult.IncrementLinksChecked();
        }

        private async Task<List<Link>> GetLinksFromPage(Link url, PageStats pageStats)
        {
            var linkList = await RequestAndProcessPage(url, pageStats);
            CrawlResult.AddVisitedPage(pageStats);
            return linkList;
        }

        private async Task<List<Link>> RequestAndProcessPage(Link url, PageStats pageStats)
        {
            await CrawlerConfig.Semaphore.WaitAsync();

            try
            {
                var (response, requestTime) = await RequestPageAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var (links, parseTime) = await Utilities.BenchmarkAsync(() => _linkExtractor.GetLinksFromResponseAsync(response, url));
                    pageStats.AddMetrics(response, requestTime, parseTime);
                    return links;
                }

                pageStats.AddMetrics(response, requestTime);

                if (response.StatusCode != HttpStatusCode.Forbidden)
                {
                    CrawlResult.AddBrokenLink(new BrokenLink(url, response.StatusCode));
                }

                return new List<Link>();
            }
            finally
            {
                CrawlerConfig.Semaphore.Release();
            }
        }

        private async Task<(HttpResponseMessage, long)> RequestPageAsync(Link url)
        {
            await CrawlerConfig.ApplyJitterAsync();
            return await Utilities.BenchmarkAsync(() => _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead));
        }
    }
}
