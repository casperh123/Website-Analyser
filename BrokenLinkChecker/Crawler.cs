using System.Diagnostics;
using HtmlAgilityPack;

namespace BrokenLinkChecker
{
    public class Crawler
    {
        private readonly string _url;
        private readonly HttpClient _httpClient;
        public CrawlerState CrawlerState { get; private set; }
        public int LinksChecked { get; private set; }

        public Crawler(string url, HttpClient httpClient, CrawlerState crawlerState)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            CrawlerState = crawlerState ?? throw new ArgumentNullException(nameof(crawlerState));
        }

        public async Task GetBrokenLinksAsync()
        {
            var linkQueue = new Queue<LinkNode>();
            linkQueue.Enqueue(new LinkNode("", _url, "", 0));
            var ongoingTasks = new List<Task>();

            while (linkQueue.Count > 0 || ongoingTasks.Count > 0)
            {
                while (linkQueue.Count > 0 && ongoingTasks.Count < 10000) // Ensure not to overload with too many tasks
                {
                    var currentLink = linkQueue.Dequeue();
                    if (currentLink == null || !CrawlerState.VisitedLinks.Add(currentLink.Target))
                    {
                        continue;
                    }

                    var task = ProcessLinkAsync(currentLink, linkQueue);
                    ongoingTasks.Add(task);
                }

                var completedTask = await Task.WhenAny(ongoingTasks);
                ongoingTasks.Remove(completedTask);
            }
        }

        private async Task ProcessLinkAsync(LinkNode url, Queue<LinkNode> linkQueue)
        {
            await CrawlerState.Semaphore.WaitAsync(); // Wait for the semaphore
            try
            {
                var links = await FetchAndParseLinksAsync(url);
                foreach (var link in links)
                {
                    if (!IsAsyncOrFragmentRequest(link.Target) && !CrawlerState.VisitedLinks.Contains(link.Target))
                    {
                        linkQueue.Enqueue(link);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error retrieving or parsing {url.Target}: {ex.Message}");
            }
            finally
            {
                CrawlerState.Semaphore.Release(); // Release the semaphore
            }
        }

        private async Task<List<LinkNode>> FetchAndParseLinksAsync(LinkNode url)
        {
            var stopwatch = Stopwatch.StartNew(); // Start timing
            var linkList = new List<LinkNode>();
            HttpResponseMessage response = null;

            try
            {
                response = await _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    CrawlerState.BrokenLinks.Add(new BrokenLink(url.Target, url.Referrer, url.AnchorText, url.Line, (int)response.StatusCode));
                    LogError($"Added to broken links list: {url.Target}");
                    return linkList;
                }

                await using var contentStream = await response.Content.ReadAsStreamAsync();
                var doc = new HtmlDocument();
                doc.Load(contentStream);

                var links = doc.DocumentNode.SelectNodes("//a[@href]");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        var href = link.GetAttributeValue("href", string.Empty);
                        var resolvedUrl = Utilities.GetUrl(url.Target, href);
                        var fullUri = new Uri(resolvedUrl);

                        // Only add links from the same domain
                        if (fullUri.Host == new Uri(url.Target).Host)
                        {
                            linkList.Add(new LinkNode(url.Target, fullUri.ToString(), link.InnerText, link.Line));
                        }
                    }
                }
            }
            catch (HttpRequestException httpRequestEx)
            {
                CrawlerState.BrokenLinks.Add(new BrokenLink(url.Target, url.Referrer, url.AnchorText, url.Line, -1, httpRequestEx.Message));
                LogError($"HttpRequestException for {url.Target}: {httpRequestEx.Message}");
            }
            catch (Exception ex)
            {
                CrawlerState.BrokenLinks.Add(new BrokenLink(url.Target, url.Referrer, url.AnchorText, url.Line, -1, ex.Message));
                LogError($"Exception for {url.Target}: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                LinksChecked++;
                LogInfo($"Checked {url.Target}, Status Code: {response?.StatusCode}, Response Time: {stopwatch.ElapsedMilliseconds} ms, Checked {LinksChecked} Links");
            }

            return linkList;
        }

        private bool IsAsyncOrFragmentRequest(string url)
        {
            // Define patterns or keywords that suggest an async request
            string[] asyncKeywords = { "ajax", "async", "action=async" };

            if (asyncKeywords.Any(keyword => url.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Check if the URL is a fragment identifier (e.g., #section)
            return url.Contains('#') || url.Contains('?');
        }

        private void LogError(string message)
        {
            Console.Error.WriteLine(message);
        }

        private void LogInfo(string message)
        {
            Console.WriteLine(message);
        }
    }
}
