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
        private readonly List<BrokenLink> _brokenLinks = [];
        private readonly ConcurrentDictionary<string, PageStats> _visitedResources = [];
        
        private CrawlerConfig CrawlerConfig { get; }
        private int LinksChecked { get; set; }

        public Action<ICollection<BrokenLink>> OnBrokenLinks;
        public Action<ICollection<PageStats>> OnPageVisited;
        public Action<int> OnLinksEnqueued;
        public Action<int> OnLinksChecked;

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            CrawlerConfig = crawlerConfig ?? throw new ArgumentNullException(nameof(crawlerConfig));
            _linkExtractor = new LinkExtractor(CrawlerConfig);
        }

        public async Task<List<PageStats>> CrawlWebsiteAsync(Uri url)
        {
            ConcurrentBag<Link> linkQueue = new () { new Link("", url.ToString(), "", 0) };

            do
            {
                OrderablePartitioner<Link> partitioner = Partitioner.Create(linkQueue);
                ConcurrentBag<Link> foundLinks = new ConcurrentBag<Link>();

                await Parallel.ForEachAsync(partitioner.GetDynamicPartitions(),
                    async (link, cancellationToken) =>
                    {
                        await ProcessLinkAsync(link, foundLinks);
                    });

                OnLinksEnqueued?.Invoke(foundLinks.Count);

                linkQueue = foundLinks;

            } while (linkQueue.Count > 0);

            return _visitedResources.Values.ToList();
        }

        private async Task ProcessLinkAsync(Link url, ConcurrentBag<Link> linksFound)
        {
            PageStats pageStats = new PageStats(url.Target, HttpStatusCode.Unused);
            
            if (!_visitedResources.TryAdd(url.Target, pageStats))
            {
                pageStats = _visitedResources[url.Target];
                
                if (pageStats.StatusCode is HttpStatusCode.OK or HttpStatusCode.Unused)
                {
                    return;
                }

                _brokenLinks.Add(new BrokenLink(url, pageStats.StatusCode));
                return;
            }

            List<Link> links = await GetLinksFromPage(url, pageStats);

            foreach (Link link in links.Where(link => !Utilities.IsAsyncOrFragmentRequest(link.Target)))
            {
                linksFound.Add(link);
            }

            OnLinksChecked(LinksChecked++);
        }

        private async Task<List<Link>> GetLinksFromPage(Link url, PageStats pageStats)
        {
            List<Link> linkList = await RequestAndProcessPage(url, pageStats);

            OnLinksChecked.Invoke(LinksChecked++);
            OnBrokenLinks.Invoke(_brokenLinks);
            OnPageVisited.Invoke(_visitedResources.Values);

            return linkList;
        }

        private async Task<List<Link>> RequestAndProcessPage(Link url, PageStats pageStats)
        {
            await CrawlerConfig.Semaphore.WaitAsync();

            (HttpResponseMessage response, long requestTime) = await RequestPageAsync(url);
                
            if (response.IsSuccessStatusCode)
            {
                (List<Link> links, long parseTime) = await Utilities.BenchmarkAsync(() => _linkExtractor.GetLinksFromResponseAsync(response, url));
                
                pageStats.AddMetrics(response, requestTime, parseTime);
                
                CrawlerConfig.Semaphore.Release();
                
                return links;
            }
            
            CrawlerConfig.Semaphore.Release();
            
            pageStats.AddMetrics(response, requestTime);
            _brokenLinks.Add(new BrokenLink(url, response.StatusCode));
            return [];
        }

        private async Task<(HttpResponseMessage, long)> RequestPageAsync(Link url)
        {
            await CrawlerConfig.ApplyJitterAsync();

            (HttpResponseMessage response, long requestTime) = await Utilities.BenchmarkAsync(() => _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead));
            
            return (response, requestTime);
        }
    }
}