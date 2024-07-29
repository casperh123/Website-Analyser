using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.crawler
{
    public class Crawler
    {
        private readonly HttpClient _httpClient;

        public readonly ConcurrentDictionary<string, PageStats> VisitedPages;
        private readonly List<BrokenLink> _brokenLinks = new();
        private CrawlerConfig CrawlerConfig { get; }

        private int LinksChecked { get; set; }

        public Action<List<BrokenLink>> OnBrokenLinks;
        public Action<int> OnLinksEnqueued;
        public Action<int> OnLinksChecked;
        public Action<int> OnTotalRequestTime;


        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            VisitedPages = new ConcurrentDictionary<string, PageStats>();
            CrawlerConfig = crawlerConfig ?? throw new ArgumentNullException(nameof(crawlerConfig));
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
                    // Check if the Content-Type header indicates an HTML document
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    if (contentType != null && contentType.Equals("text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        Stopwatch documentParseTiming = Stopwatch.StartNew();
                        await using Stream document = await response.Content.ReadAsStreamAsync();
                        linkList = await ExtractLinksFromDocumentAsync(document, url);
                        stats.DocumentParseTime = documentParseTiming.ElapsedMilliseconds;
                    }
                    else
                    {
                        // Log or handle non-HTML content
                        Console.WriteLine($"Skipped non-HTML content at {url.Target}");
                    }
                }

                VisitedPages[url.Target].StatusCode = response.StatusCode;
            }

            OnLinksChecked.Invoke(LinksChecked++);
            OnBrokenLinks.Invoke(_brokenLinks);

            return linkList;
        }

        private async Task<List<LinkNode>> ExtractLinksFromDocumentAsync(Stream document, LinkNode checkingUrl)
        {
            List<LinkNode> links = new List<LinkNode>();
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            IHtmlParser parser = context.GetService<IHtmlParser>() ?? new HtmlParser();

            IDocument doc = await parser.ParseDocumentAsync(document);
            IHtmlCollection<IElement> documentLinks = doc.QuerySelectorAll("a[href]");

            foreach (IElement link in documentLinks)
            {
                string href = link.GetAttribute("href");
                if (!string.IsNullOrEmpty(href))
                {
                    LinkNode newLink = GenerateLinkNode(link, checkingUrl.Target);
                    if (Uri.TryCreate(newLink.Target, UriKind.Absolute, out Uri uri) && uri.Host == new Uri(checkingUrl.Target).Host)
                    {
                        links.Add(newLink);
                    }
                }
            }

            return links;
        }

        
        private LinkNode GenerateLinkNode(IElement link, string target)
        {
            string href = link.GetAttribute("href") ?? string.Empty;
            string resolvedUrl;
            try
            {
                resolvedUrl = Utilities.GetUrl(target, href);
            }
            catch (UriFormatException ex)
            {
                resolvedUrl = href; // Fall back to original href or handle as needed
            }
            string text = link.TextContent;
            int line = link.SourceReference?.Position.Line ?? -1;

            return new LinkNode(target, resolvedUrl, text, line);
        }
        
        private async Task ApplyJitterAsync()
        {
            await Task.Delay(new Random().Next(1000));
        }
    }
}