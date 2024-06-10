using System.Collections.Concurrent;
using System.Net;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;
using HtmlAgilityPack;
using Serilog;

namespace BrokenLinkChecker.crawler
{
    public class Crawler
    {
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, HttpStatusCode> _visitedPages;

        private readonly List<BrokenLink> _brokenLinks = new();
        private CrawlerConfig CrawlerConfig { get; }

        private int LinksChecked { get; set; }

        // Delegates to notify when links are enqueued and checked
        public Action<List<BrokenLink>> OnBrokenLinks;
        public Action<int> OnLinksEnqueued;
        public Action<int> OnLinksChecked;

        public Crawler(HttpClient httpClient, CrawlerConfig crawlerConfig)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _visitedPages = new ConcurrentDictionary<string, HttpStatusCode>();
            CrawlerConfig = crawlerConfig ?? throw new ArgumentNullException(nameof(crawlerConfig));
        }

        public async Task<List<BrokenLink>> GetBrokenLinksAsync(Uri url)
        {
            Queue<LinkNode> linkQueue = new();
            linkQueue.Enqueue(new LinkNode("", url.ToString(), "", 0));
            List<Task> ongoingTasks = new();

            while (linkQueue.Count > 0 || ongoingTasks.Count > 0)
            {
                while (linkQueue.Count > 0 && ongoingTasks.Count < 100) // Ensure not to overload with too many tasks
                {
                    LinkNode currentLink = linkQueue.Dequeue();

                    if (_visitedPages.ContainsKey(currentLink.Target))
                    {
                        continue;
                    }

                    Task task = ProcessLinkAsync(currentLink, linkQueue);
                    ongoingTasks.Add(task);
                }

                if (ongoingTasks.Count > 0)
                {
                    Task completedTask = await Task.WhenAny(ongoingTasks);
                    ongoingTasks.Remove(completedTask);
                }
            }

            foreach (var key in _visitedPages)
            {
                Console.WriteLine($"{key}");
            }

            return _brokenLinks;
        }

        private async Task ProcessLinkAsync(LinkNode url, Queue<LinkNode> linkQueue)
        {
            try
            {
                await CrawlerConfig.Semaphore.WaitAsync();
                List<LinkNode> links = await FetchAndParseLinksAsync(url);

                foreach (LinkNode link in links.Where(link => !Utilities.IsAsyncOrFragmentRequest(link.Target)))
                {
                    linkQueue.Enqueue(link);
                    OnLinksEnqueued?.Invoke(linkQueue.Count);  // Notify Blazor component
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing link {Target}", url.Target);
            }
            finally
            {
                OnLinksChecked?.Invoke(LinksChecked++);  // Notify Blazor component
                CrawlerConfig.Semaphore.Release(); // Release the semaphore
            }
        }

        private async Task<List<LinkNode>> FetchAndParseLinksAsync(LinkNode url)
        {
            List<LinkNode> linkList = new();

            // Check the page Cache
            if (_visitedPages.TryGetValue(url.Target, out HttpStatusCode statusCode))
            {
                Log.Information("Cache hit for {Target}", url.Target);
                
                // Already being processed
                if (statusCode is HttpStatusCode.Unused or HttpStatusCode.OK)
                {
                    return linkList;
                }

                // Cache Hit
                _brokenLinks.Add(new BrokenLink(url.Target, url.Referrer, url.AnchorText, url.Line, (int)statusCode));
                Log.Information("Cache hit for {Target}", url.Target);
            }
            else
            {
                _visitedPages[url.Target] = HttpStatusCode.Unused;

                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                    {
                        _brokenLinks.Add(new BrokenLink(url.Target, url.Referrer, url.AnchorText, url.Line, (int)response.StatusCode));
                    }
                    else
                    {
                        await using Stream contentStream = await response.Content.ReadAsStreamAsync();
                        HtmlDocument doc = new HtmlDocument();
                        await Task.Run(() => doc.Load(contentStream)); // Load document asynchronously

                        HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a[@href]");

                        if (links != null)
                        {
                            foreach (HtmlNode link in links)
                            {
                                LinkNode newLink = GenerateLinkNode(link, url.Target);

                                if (new Uri(newLink.Target).Host == new Uri(url.Target).Host)
                                {
                                    linkList.Add(newLink);
                                }
                            }
                        }
                    }
                    _visitedPages[url.Target] = response.StatusCode;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error fetching link {Target}", url.Target);
                    _brokenLinks.Add(new BrokenLink(url.Target, url.Referrer, url.AnchorText, url.Line, -1, ex.Message));
                }
            }

            OnLinksChecked?.Invoke(LinksChecked++);
            OnBrokenLinks?.Invoke(_brokenLinks);

            return linkList;
        }

        private LinkNode GenerateLinkNode(HtmlNode link, string target)
        {
            string href = link.GetAttributeValue("href", string.Empty);
            Uri resolvedUrl = Utilities.GetUrl(target, href);

            return new LinkNode(target, resolvedUrl.ToString(), link.InnerText, link.Line);
        }
    }
}
