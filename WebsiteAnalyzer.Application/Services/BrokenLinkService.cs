using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.Models.Links;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class BrokenLinkService : IBrokenLinkService
{
    private readonly HttpClient _httpClient;

    public BrokenLinkService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task FindBrokenLinks(
        string url, 
        Action<int> onLinkEnqueued, 
        Action<int> onLinkChecked,
        Action<IndexedLink> onLinkFound, 
        CancellationToken cancellationToken)
    {
        ILinkProcessor<IndexedLink> linkProcessor = new BrokenLinkProcessor(_httpClient);
        ModularCrawler<IndexedLink> crawler = new(linkProcessor);

        crawler.OnLinksEnqueued += onLinkEnqueued;
        crawler.OnLinksChecked += onLinkChecked;
        crawler.OnResourceVisited += onLinkFound;

        await crawler.CrawlWebsiteAsync(new IndexedLink(string.Empty, url, "", 0), cancellationToken);
    }
}