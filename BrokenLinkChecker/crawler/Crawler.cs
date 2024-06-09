using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;
using HtmlAgilityPack;

namespace BrokenLinkChecker.crawler
{
    public class Crawler
    {
        private readonly HttpClient _httpClient;
        public CrawlerState CrawlerState { get; private set; }
        public int LinksChecked { get; private set; }
        public int LinksEnqueued { get; private set; }

        // Delegates to notify when links are enqueued and checked
        public Action<int> OnLinksEnqueued;
        public Action<int> OnLinksChecked;

        public Crawler(HttpClient httpClient, CrawlerState crawlerState)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            CrawlerState = crawlerState ?? throw new ArgumentNullException(nameof(crawlerState));
        }

        public async Task GetBrokenLinksAsync(string url)
        {
            Console.WriteLine("GetBrokenLinksAsync started.");
            Queue<LinkNode> linkQueue = new Queue<LinkNode>();
            linkQueue.Enqueue(new LinkNode("", url, "", 0));
            LinksEnqueued++;  // Initial link enqueue
            OnLinksEnqueued?.Invoke(LinksEnqueued);  // Notify Blazor component
            List<Task> ongoingTasks = new List<Task>();

            while (linkQueue.Count > 0 || ongoingTasks.Count > 0)
            {
                while (linkQueue.Count > 0 && ongoingTasks.Count < 500) // Ensure not to overload with too many tasks
                {
                    LinkNode? currentLink = linkQueue.Dequeue();
                    if (currentLink == null || !CrawlerState.VisitedLinks.Add(currentLink.Target))
                    {
                        continue;
                    }

                    Task task = ProcessLinkAsync(currentLink, linkQueue);
                    ongoingTasks.Add(task);
                }

                Task completedTask = await Task.WhenAny(ongoingTasks);
                ongoingTasks.Remove(completedTask);
            }
            Console.WriteLine("GetBrokenLinksAsync completed.");
        }

        private async Task ProcessLinkAsync(LinkNode url, Queue<LinkNode> linkQueue)
        {
            await CrawlerState.Semaphore.WaitAsync();

            List<LinkNode> links = await FetchAndParseLinksAsync(url);
            foreach (LinkNode link in links.Where(link => !IsAsyncOrFragmentRequest(link.Target) && !CrawlerState.VisitedLinks.Contains(link.Target)))
            {
                linkQueue.Enqueue(link);
                LinksEnqueued++;
                OnLinksEnqueued?.Invoke(LinksEnqueued);  // Notify Blazor component
            }

            LinksChecked++;
            OnLinksChecked?.Invoke(LinksChecked);  // Notify Blazor component

            CrawlerState.Semaphore.Release(); // Release the semaphore
        }

        private async Task<List<LinkNode>> FetchAndParseLinksAsync(LinkNode url)
        {
            Stopwatch stopwatch = Stopwatch.StartNew(); // Start timing
            List<LinkNode> linkList = new List<LinkNode>();
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

                await using Stream contentStream = await response.Content.ReadAsStreamAsync();
                HtmlDocument doc = new HtmlDocument();
                doc.Load(contentStream);

                HtmlNodeCollection? links = doc.DocumentNode.SelectNodes("//a[@href]");

                if (links == null)
                {
                    return linkList;
                }

                foreach (HtmlNode link in links)
                {
                    LinkNode newLink = GenerateLinkNode(link, url.Target);

                    if (new Uri(newLink.Target).Host == new Uri(url.Target).Host)
                    {
                        linkList.Add(newLink);
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
                OnLinksChecked?.Invoke(LinksChecked);  // Notify Blazor component
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

        private LinkNode GenerateLinkNode(HtmlNode link, string target)
        {
            string? href = link.GetAttributeValue("href", string.Empty);
            Uri resolvedUrl = Utilities.GetUrl(target, href);

            return new LinkNode(target, resolvedUrl.ToString(), link.InnerText, link.Line);
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
