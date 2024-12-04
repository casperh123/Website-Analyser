using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.Models.Links;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface IBrokenLinkService
{
    Task FindBrokenLinks(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked,
        Action<IndexedLink> onBrokenLinkFound);
}

public class BrokenLinkService : IBrokenLinkService
{
    private readonly HttpClient _httpClient;

    public BrokenLinkService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task FindBrokenLinks(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked,
        Action<IndexedLink> onBrokenLinkFound)
    {
        ModularCrawlResult<IndexedLink> crawlResult =
            GenerateCrawlResult(onLinkEnqueued, onLinkChecked, onBrokenLinkFound);
        ILinkProcessor<IndexedLink> linkProcessor = new BrokenLinkProcessor(_httpClient);
        ModularCrawler<IndexedLink> crawler = new(linkProcessor);

        await crawler.CrawlWebsiteAsync(new IndexedLink(string.Empty, url, "", 0), crawlResult);
    }

    private ModularCrawlResult<IndexedLink> GenerateCrawlResult(Action<int> onLinkEnqueued, Action<int> onLinkChecked,
        Action<IndexedLink> onBrokenLinkFound)
    {
        ModularCrawlResult<IndexedLink> crawlResult = new();

        crawlResult.OnLinksEnqueued += onLinkEnqueued;
        crawlResult.OnLinksChecked += onLinkChecked;
        crawlResult.OnResouceVisited += onBrokenLinkFound;

        return crawlResult;
    }
}