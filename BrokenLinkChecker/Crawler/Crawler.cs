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
        private readonly ConcurrentDictionary<string, HttpStatusCode> _visitedResources = new();
        
        private CrawlerConfig CrawlerConfig { get; }
        private CrawlResult CrawlResult { get; }

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig, CrawlResult crawlResult)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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
                if (_visitedResources[url.Target] is HttpStatusCode status && status != HttpStatusCode.OK && status != HttpStatusCode.Forbidden)
                {
                    CrawlResult.AddBrokenLink(new BrokenLink(url, status));
                }
                return;
            }

            List<Link> links = await GetLinksFromPage(url);

            foreach (Link link in links.Where(link => !Utilities.IsAsyncOrFragmentRequest(link.Target)))
            {
                linksFound.Enqueue(link);
            }

            CrawlResult.IncrementLinksChecked();
        }



        private async Task<List<Link>> GetLinksFromPage(Link url)
        {
            List<Link> linkList = await RequestAndProcessPage(url);
            return linkList;
        }

        private async Task<List<Link>> RequestAndProcessPage(Link url)
        {
            try
            {
                await CrawlerConfig.Semaphore.WaitAsync();

                (HttpResponseMessage response, long requestTime) = await RequestPageAsync(url);

                _visitedResources[url.Target] = response.StatusCode;

                return await ProcessResponse(response, url, requestTime);
            }
            finally
            {
                CrawlerConfig.Semaphore.Release();
            }
        }
        
        private async Task<List<Link>> ProcessResponse(HttpResponseMessage response, Link url, long requestTime)
        {
            if (response.IsSuccessStatusCode)
            {
                (List<Link> links, long parseTime) = await Utilities.BenchmarkAsync(() => _linkExtractor.GetLinksFromResponseAsync(response, url));

                PageStat pageStat = new PageStat(url.Target, response, url.Type, requestTime, parseTime);
                CrawlResult.AddVisitedPage(pageStat);

                return links;
            }

            HandleBrokenLink(url, response);

            return [];
        }

        private void HandleBrokenLink(Link url, HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.Forbidden)
            {
                CrawlResult.AddBrokenLink(new BrokenLink(url, response.StatusCode));
            }
        }

        private async Task<(HttpResponseMessage, long)> RequestPageAsync(Link url)
        {
            await CrawlerConfig.ApplyJitterAsync();
            return await Utilities.BenchmarkAsync(() => _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead));
        }
    }
}