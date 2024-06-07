using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    private static HttpClient httpClient = new HttpClient(
        new HttpClientHandler()
        {
            UseCookies = false
        });
    private static HashSet<string> visitedLinks = new HashSet<string>();
    private static List<string> notFoundLinks = new List<string>();
    private static SemaphoreSlim semaphore = new SemaphoreSlim(4); // Control concurrency, 10 tasks at a time

    static async Task Main(string[] args)
    {
        ServicePointManager.DefaultConnectionLimit = 100;   
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
        Queue<string> linkQueue = new Queue<string>();
        linkQueue.Enqueue(baseUrl);
        List<Task> ongoingTasks = new List<Task>();

        while (linkQueue.Count > 0 || ongoingTasks.Count > 0)
        {
            while (linkQueue.Count > 0 && ongoingTasks.Count < 100) // Ensure not to overload with too many tasks
            {
                string currentUrl = linkQueue.Dequeue();
                if (!visitedLinks.Contains(currentUrl))
                {
                    visitedLinks.Add(currentUrl);
                    var task = ProcessLink(currentUrl, linkQueue);
                    ongoingTasks.Add(task);
                }
            }

            var completedTask = await Task.WhenAny(ongoingTasks);
            ongoingTasks.Remove(completedTask);
        }
    }

    static async Task ProcessLink(string url, Queue<string> linkQueue)
    {
        await semaphore.WaitAsync(); // Wait for the semaphore
        try
        {
            var links = await FetchAndParseLinks(url);
            foreach (var link in links)
            {
                if (!visitedLinks.Contains(link))
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

    static async Task<List<string>> FetchAndParseLinks(string url)
    {
        var linkList = new List<string>();
        var response = await httpClient.GetAsync(url);
        Console.WriteLine($"Checked {url}, Status Code: {response.StatusCode}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            notFoundLinks.Add(url);
            Console.WriteLine($"Added to 404 list: {url}");
        }
        else if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                Uri baseUri = new Uri(url);
                foreach (var link in links)
                {
                    var href = link.Attributes["href"].Value;
                    Uri fullUri;
                    if (Uri.TryCreate(href, UriKind.Absolute, out fullUri))
                    {
                        // Only add links from the same domain
                        if (fullUri.Host == baseUri.Host)
                        {
                            linkList.Add(fullUri.ToString());
                        }
                    }
                    else
                    {
                        // Resolve relative links
                        var resolvedUri = new Uri(baseUri, href);
                        if (resolvedUri.Host == baseUri.Host)
                        {
                            linkList.Add(resolvedUri.ToString());
                        }
                    }
                }
            }
        }

        return linkList;
    }
}

public class LinkNode(string referrer, string target, HtmlNode referringNode)
{
    public string Referrer { get; set; } = referrer;

    public string Target { get; set; } = target;

    public HtmlNode ReferringNode = referringNode;
}

public class BrokenLink
{
    public string Url { get; set; }
    public string ReferringPage { get; set; }
    public string AnchorText { get; set; }

    public BrokenLink(string url, string referringPage, string anchorText)
    {
        Url = url;
        ReferringPage = referringPage;
        AnchorText = anchorText;
    }

    public override string ToString()
    {
        return $"Broken Link Found: URL={Url}, Anchor Text='{AnchorText}', Found on Page={ReferringPage}";
    }
}
