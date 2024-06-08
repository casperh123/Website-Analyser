using System.Diagnostics;
using System.Net;
using BrokenLinkChecker;
using HtmlAgilityPack;

Crawler crawler = new Crawler("https://adidas.dk", HttpClientSingleton.Instance, new CrawlerState(8));
       
ServicePointManager.DefaultConnectionLimit = 1000; // Increase the concurrent connections limit
ServicePointManager.Expect100Continue = false; // Enhance performance for POST requests
        
const string baseUrl = "https://adidas.dk";
        
Console.WriteLine("Starting comprehensive link check at: " + baseUrl);

await crawler.GetBrokenLinksAsync();

Console.WriteLine("Broken Links:");
if (crawler.CrawlerState.BrokenLinks.Count == 0)
{
    Console.WriteLine("No broken links found.");
}
else
{
    crawler.CrawlerState.BrokenLinks.ForEach(Console.WriteLine);
}