using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.Models.Links;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface IBrokenLinkService
{
    Task FindBrokenLinks(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked,
        Action<IndexedLink> onLinkFound);
}

public class BrokenLinkService : IBrokenLinkService
{
    private readonly HttpClient _httpClient;

    public BrokenLinkService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task FindBrokenLinks(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked,
        Action<IndexedLink> onLinkFound)
    {
        
        ILinkProcessor<IndexedLink> linkProcessor = new BrokenLinkProcessor(_httpClient);
        ModularCrawler<IndexedLink> crawler = new(linkProcessor);
        
        crawler.OnLinksEnqueued += onLinkEnqueued;
        crawler.OnLinksChecked += onLinkChecked;
        crawler.OnResourceVisited += onLinkFound;

        await crawler.CrawlWebsiteAsync(new IndexedLink(string.Empty, url, "", 0));
    }
}