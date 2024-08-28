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
        private readonly List<BrokenLink> _brokenLinks = [];
        private readonly LinkExtractor _linkExtractor;
        private CrawlerConfig CrawlerConfig { get; }
        private int LinksChecked { get; set; }
        private readonly ConcurrentDictionary<string, PageStats> _visitedPages;
        
        public Action<List<BrokenLink>> OnBrokenLinks;
        public Action<ICollection<PageStats>> OnPageVisited;
        public Action<int> OnLinksEnqueued;
        public Action<int> OnLinksChecked;

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _visitedPages = new ConcurrentDictionary<string, PageStats>();
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

            return _visitedPages.Values.ToList();
        }

        private async Task ProcessLinkAsync(Link url, ConcurrentBag<Link> linksFound)
        {
            PageStats pageStats = new PageStats(url.Target, HttpStatusCode.Unused);
            
            if (!_visitedPages.TryAdd(url.Target, pageStats))
            {
                pageStats = _visitedPages[url.Target];
                
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
            OnPageVisited.Invoke(_visitedPages.Values);

            return linkList;
        }

        private async Task<List<Link>> RequestAndProcessPage(Link url, PageStats pageStats)
        {
            (HttpResponseMessage response, long requestTime) = await RequestPageAsync(url);
                
            if (response.IsSuccessStatusCode)
            {
                (List<Link> links, long parseTime) = await Utilities.BenchmarkAsync(() => _linkExtractor.GetLinksFromResponseAsync(response, url));
     
                pageStats.AddMetrics(response, requestTime, parseTime);
                
                return links;
            }
            
            pageStats.AddMetrics(response, requestTime);
            _brokenLinks.Add(new BrokenLink(url, response.StatusCode));
            return [];
        }

        private async Task<(HttpResponseMessage, long)> RequestPageAsync(Link url)
        {
            await CrawlerConfig.Semaphore.WaitAsync();
            await CrawlerConfig.ApplyJitterAsync();

            (HttpResponseMessage response, long requestTime) = await Utilities.BenchmarkAsync(() => _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead));
            
            CrawlerConfig.Semaphore.Release();

            return (response, requestTime);
        }
    }
}