using System.Diagnostics;
using System.Net;
using BrokenLinkChecker;
using BrokenLinkChecker.crawler;
using BrokenLinkChecker.Utility;
using HtmlAgilityPack;

string url = "https://skadedyrsexperten.dk";

Crawler crawler = new Crawler(HttpClientSingleton.Instance, new CrawlerState(16));

Console.WriteLine("Starting comprehensive link check at: " + url);

await crawler.GetBrokenLinksAsync(url);

Console.WriteLine("Broken Links:");
if (crawler.CrawlerState.BrokenLinks.Count == 0)
{
    Console.WriteLine("No broken links found.");
}
else
{
    crawler.CrawlerState.BrokenLinks.ForEach(Console.WriteLine);
}