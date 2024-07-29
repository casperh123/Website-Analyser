using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using AngleSharp;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.crawler
{
    public class Crawler
    {
        private readonly HttpClient _httpClient;

        public readonly ConcurrentDictionary<string, PageStats> VisitedPages;
        private readonly List<BrokenLink> _brokenLinks = new();
        private LinkExtractor _linkExtractor;
        private CrawlerConfig CrawlerConfig { get; }

        private int LinksChecked { get; set; }

        public Action<List<BrokenLink>> OnBrokenLinks;
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
            List<LinkNode> linkQueue = new List<LinkNode> { new LinkNode("", url.ToString(), "", 0) };
            ConcurrentBag<LinkNode> foundLinks = new ();

            do
            {
                var partitioner = Partitioner.Create(linkQueue);

                await Parallel.ForEachAsync(partitioner.GetDynamicPartitions(),
                    async (link, cancellationToken) => { await ProcessLinkAsync(link, foundLinks); });

                OnLinksEnqueued.Invoke(foundLinks.Count);

                linkQueue = foundLinks.ToList();
                foundLinks = new ConcurrentBag<LinkNode>();

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
            List<LinkNode> linkList = new();
            
            // Check the page Cache
            if (VisitedPages.TryGetValue(url.Target, out PageStats pageStats))
            {
                if (pageStats.StatusCode is HttpStatusCode.OK or HttpStatusCode.Unused)
                {
                    return linkList;
                }

                _brokenLinks.Add(new BrokenLink(url.Target, url.Referrer, url.AnchorText, url.Line, (int)pageStats.StatusCode));
            }
            else
            {
                PageStats stats = new PageStats(url.Target, HttpStatusCode.Unused);
                VisitedPages[url.Target] = stats;
                
                await CrawlerConfig.Semaphore.WaitAsync();
                await ApplyJitterAsync();

                Stopwatch responseTiming = Stopwatch.StartNew();

                using HttpResponseMessage response = await _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead);

                stats.ResponseTime = responseTiming.ElapsedMilliseconds;
                
                CrawlerConfig.Semaphore.Release();
                
                if (!response.IsSuccessStatusCode)
                {
                    _brokenLinks.Add(new BrokenLink(
                        url.Target, 
                        url.Referrer, 
                        url.AnchorText, 
                        url.Line, 
                        (int)response.StatusCode));
                }
                else
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    // Check if the Content-Type header indicates an HTML document
                    linkList = await _linkExtractor.GetLinksFromResponseAsync(response, url);
                    stats.DocumentParseTime = stopwatch.ElapsedMilliseconds;
                }

                VisitedPages[url.Target].StatusCode = response.StatusCode;
            }

            OnLinksChecked.Invoke(LinksChecked++);
            OnBrokenLinks.Invoke(_brokenLinks);

            return linkList;
        }
        
        private async Task ApplyJitterAsync()
        {
            await Task.Delay(new Random().Next(1000));
        }
    }
}