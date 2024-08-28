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
        
        public readonly ConcurrentDictionary<string, PageStats> VisitedPages;
        public Action<List<BrokenLink>> OnBrokenLinks;
        public Action<ICollection<PageStats>> OnPageVisited;
        public Action<int> OnLinksEnqueued;
        public Action<int> OnLinksChecked;

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            VisitedPages = new ConcurrentDictionary<string, PageStats>();
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

            return VisitedPages.Values.ToList();
        }

        private async Task ProcessLinkAsync(Link url, ConcurrentBag<Link> linksFound)
        {
            if (VisitedPages.ContainsKey(url.Target))
            {
                return;
            }

            List<Link> links = await GetLinksFromPage(url);

            foreach (Link link in links.Where(link => !Utilities.IsAsyncOrFragmentRequest(link.Target)))
            {
                linksFound.Add(link);
            }

            OnLinksChecked.Invoke(LinksChecked++);
        }

        private async Task<List<Link>> GetLinksFromPage(Link url)
        {
            List<Link> linkList = [];
            
            if (VisitedPages.TryGetValue(url.Target, out PageStats pageStats))
            {
                if (pageStats.StatusCode is HttpStatusCode.OK or HttpStatusCode.Unused)
                {
                    return linkList;
                }

                _brokenLinks.Add(new BrokenLink(url, pageStats.StatusCode));
            }
            else
            {
                linkList = await RequestAndProcessPage(url);
            }

            OnLinksChecked.Invoke(LinksChecked++);
            OnBrokenLinks.Invoke(_brokenLinks);
            OnPageVisited.Invoke(VisitedPages.Values);

            return linkList;
        }

        private async Task<List<Link>> RequestAndProcessPage(Link url)
        {
            PageStats pageStats = new PageStats(url.Target, HttpStatusCode.Unused);
            VisitedPages[url.Target] = pageStats;
            
            (HttpResponseMessage response, long requestTime) = await RequestPageAsync(url);
                
            if (response.IsSuccessStatusCode)
            {
                (List<Link> links, long parseTime) = await Utilities.BenchmarkAsync(() => _linkExtractor.GetLinksFromResponseAsync(response, url));
     
                pageStats.AddMetrics(pageStats, response, requestTime, parseTime);
                
                return links;
            }
            
            pageStats.AddMetrics(pageStats, response, requestTime);
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