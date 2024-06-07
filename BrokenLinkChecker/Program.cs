using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BrokenLinkChecker;
using HtmlAgilityPack;

class Program
{
    private static HttpClient httpClient = new HttpClient(
        new HttpClientHandler()
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
            
        });
    private static HashSet<string> visitedLinks = new HashSet<string>();
    private static List<BrokenLink> notFoundLinks = new List<BrokenLink>();
    private static SemaphoreSlim semaphore = new SemaphoreSlim(8); // Control concurrency, 10 tasks at a time

    static async Task Main(string[] args)
    {
        ServicePointManager.DefaultConnectionLimit = 1000; // Increase the concurrent connections limit
        ServicePointManager.Expect100Continue = false; // This can enhance performance when you know that your POST requests don't need to expect a 100-Continue response from the server.
        httpClient.DefaultRequestHeaders.ConnectionClose = false;
        var baseUrl = "https://skadedyrsexperten.dk/";
        Console.WriteLine("Starting comprehensive link check at: " + baseUrl);

        await TraverseSite(baseUrl);

        Console.WriteLine("404 Not Found Links:");
        if (notFoundLinks.Count == 0)
        {
            Console.WriteLine("No broken links found.");
        }
        else
        {
            foreach (var link in notFoundLinks)
            {
                Console.WriteLine(link);
            }
        }
    }

    static async Task TraverseSite(string baseUrl)
    {
        Queue<LinkNode> linkQueue = new Queue<LinkNode>();
        linkQueue.Enqueue(new LinkNode("", baseUrl, "", 0));
        List<Task> ongoingTasks = new List<Task>();

        while (linkQueue.Count > 0 || ongoingTasks.Count > 0)
        {
            while (linkQueue.Count > 0 && ongoingTasks.Count < 80) // Ensure not to overload with too many tasks
            {
                LinkNode currentLink = linkQueue.Dequeue();
                if (currentLink == null)
                {
                    continue;
                }
                if (!visitedLinks.Contains(currentLink.Target))
                {
                    visitedLinks.Add(currentLink.Target);
                    var task = ProcessLink(currentLink, linkQueue);
                    ongoingTasks.Add(task);
                }
            }

            var completedTask = await Task.WhenAny(ongoingTasks);
            ongoingTasks.Remove(completedTask);
        }
    }

    static async Task ProcessLink(LinkNode url, Queue<LinkNode> linkQueue)
    {
        await semaphore.WaitAsync(); // Wait for the semaphore
        try
        {
            var links = await FetchAndParseLinks(url);
            foreach (var link in links)
            {
                if (!visitedLinks.Contains(link.Target))
                {
                    linkQueue.Enqueue(link);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving or parsing {url}: {ex.Message}");
        }
        finally
        {
            semaphore.Release(); // Release the semaphore
        }
    }

    public static async Task<List<LinkNode>> FetchAndParseLinks(LinkNode url)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start timing
        var linkList = new List<LinkNode>();
        HttpResponseMessage response = await httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead);

        if (response.IsSuccessStatusCode)
        {
            await using (Stream contentStream = await response.Content.ReadAsStreamAsync())
            {
                var doc = new HtmlDocument();
                doc.Load(contentStream);

                var links = doc.DocumentNode.SelectNodes("//a[@href]");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        var href = link.GetAttributeValue("href", string.Empty);
                        var resolvedUrl = Utilities.GetUrl(url.Target, href);
                        Uri fullUri = new Uri(resolvedUrl);

                        // Only add links from the same domain
                        if (fullUri.Host == new Uri(url.Target).Host)
                        {
                            linkList.Add(new LinkNode(url.Target, fullUri.ToString(), link.InnerText, link.Line));
                        }
                    }
                }
            }
            
        }
        else {
            notFoundLinks.Add(new BrokenLink(url.Target, url.Referrer, url.AnchorText, url.Line));
            Console.WriteLine($"Added to 404 list: {url.Target}");
        }
        stopwatch.Stop();
        Console.WriteLine($"Checked {url.Target}, Status Code: {response.StatusCode}, Response Time: {stopwatch.ElapsedMilliseconds} ms");

        return linkList;
    }
}

public class LinkNode
{
    public string Referrer { get; set; }
    public string Target { get; set; }
    public string AnchorText { get; set; }
    public int Line { get; set; }

    public LinkNode(string referrer, string target, string anchorText, int line)
    {
        Referrer = referrer ?? "";
        Target = target ?? "";
        AnchorText = anchorText ?? "";
        Line = line;
    }
}

public class BrokenLink(string url, string referringPage, string anchorText, int line)
{
    public string Url { get; set; } = url;

    public string ReferringPage { get; set; } = referringPage;

    public string AnchorText { get; set; } = anchorText;

    public int Line { get; set; } = line;

    public override string ToString()
    {
        return $"Broken Link Found: TARGET={Url}, ANCHOR TEXT='{AnchorText}', REFERRER={ReferringPage}, LINE:{Line}";
    }
}
