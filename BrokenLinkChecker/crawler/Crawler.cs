using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using AngleSharp;
using BrokenLinkChecker.Linkextraction;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.crawler
{
    public class Crawler
    {
        public readonly ConcurrentDictionary<string, PageStats> VisitedPages;
        private readonly HttpClient _httpClient;
        
        private readonly List<BrokenLink> _brokenLinks = [];
        
        private readonly LinkExtractor _linkExtractor;
        private CrawlerConfig CrawlerConfig { get; }

        private int LinksChecked { get; set; }

        public Action<List<BrokenLink>> OnBrokenLinks;
        public Action<ICollection<PageStats>> OnPageVisited;
        public Action<int> OnLinksEnqueued;
        public Action<int> OnLinksChecked;

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            VisitedPages = new ConcurrentDictionary<string, PageStats>();
            CrawlerConfig = crawlerConfig ?? throw new ArgumentNullException(nameof(crawlerConfig));

            _linkExtractor = new LinkExtractor(Configuration.Default);
        }

        public async Task<List<PageStats>> CrawlWebsiteAsync(Uri url)
        {
            List<LinkNode> linkQueue = [new LinkNode("", url.ToString(), "", 0)];
            ConcurrentBag<LinkNode> foundLinks = [];

            do
            {
                var partitioner = Partitioner.Create(linkQueue);

                await Parallel.ForEachAsync(partitioner.GetDynamicPartitions(),
                    async (link, cancellationToken) => { await ProcessLinkAsync(link, foundLinks); });

                OnLinksEnqueued.Invoke(foundLinks.Count);

                linkQueue = foundLinks.ToList();
                foundLinks = [];

            } while (linkQueue.Count > 0);

            return VisitedPages.Values.ToList();
        }


        private async Task ProcessLinkAsync(LinkNode url, ConcurrentBag<LinkNode> linksFound)
        {
            if (VisitedPages.ContainsKey(url.Target))
            {
                return;
            }

            List<LinkNode> links = await GetLinksFromPage(url);

            foreach (LinkNode link in links.Where(link => !Utilities.IsAsyncOrFragmentRequest(link.Target)))
            {
                linksFound.Add(link);
            }

            OnLinksChecked.Invoke(LinksChecked++); // Notify Blazor component
        }

        private async Task<List<LinkNode>> GetLinksFromPage(LinkNode url)
        {
            List<LinkNode> linkList = [];
            
            if (VisitedPages.TryGetValue(url.Target, out PageStats pageStats))
            {
                if (pageStats.StatusCode is HttpStatusCode.OK or HttpStatusCode.Unused)
                {
                    return linkList;
                }

                RecordBrokenLink(url, pageStats.StatusCode);
            }
            else
            {
                linkList = await FetchAndProcessPage(url);
            }

            OnLinksChecked.Invoke(LinksChecked++);
            OnBrokenLinks.Invoke(_brokenLinks);
            OnPageVisited.Invoke(VisitedPages.Values);

            return linkList;
        }

        private async Task<List<LinkNode>> FetchAndProcessPage(LinkNode url)
        {
            PageStats pageStats = new PageStats(url.Target, HttpStatusCode.Unused);
            
            VisitedPages[url.Target] = pageStats;

            var (response, requestTime) = await GetPageAsync(url);

            pageStats.Headers = Utilities.AddRequestHeaders(response);
            pageStats.ResponseTime = requestTime;
                
            if (response.IsSuccessStatusCode)
            {
                var (links, time) = await Utilities.BenchmarkAsync(() => _linkExtractor.GetLinksFromResponseAsync(response, url));
                pageStats.DocumentParseTime = time;
                VisitedPages[url.Target].StatusCode = response.StatusCode;
                
                return links;
            }
            
            VisitedPages[url.Target].StatusCode = response.StatusCode;
            RecordBrokenLink(url, response.StatusCode);
            return [];
        }

        private async Task<(HttpResponseMessage, long)> GetPageAsync(LinkNode url)
        {
            await CrawlerConfig.Semaphore.WaitAsync();
            
            if (CrawlerConfig.Jitter)
            {
                await ApplyJitterAsync();
            }

            var (response, requestTime) = await Utilities.BenchmarkAsync(() => _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead));
            
            CrawlerConfig.Semaphore.Release();

            return (response, requestTime);
        }
        
        private void RecordBrokenLink(LinkNode url, HttpStatusCode statusCode)
        {
            _brokenLinks.Add(new BrokenLink(url.Target, url.Referrer, url.AnchorText, url.Line, (int)statusCode));
        }
        
        private async Task ApplyJitterAsync()
        {
            await Task.Delay(new Random().Next(1000));
        }
    }
}