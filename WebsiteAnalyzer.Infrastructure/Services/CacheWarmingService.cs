using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface ICacheWarmingService
{
    Task WarmCacheAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked);
}

public class CacheWarmingService : ICacheWarmingService
{
    private HttpClient _httpClient;
    private ModularCrawler<Link>? _crawler;

    public CacheWarmingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task WarmCacheAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked)
    {
        ILinkProcessor<Link> linkProcessor = new LinkProcessor(_httpClient);
        ModularCrawlResult<Link> crawlResult = GenerateCrawlResult(onLinkEnqueued, onLinkChecked);
        ModularCrawler<Link> crawler = new ModularCrawler<Link>(linkProcessor);
            
        await crawler.CrawlWebsiteAsync(new Link(url), crawlResult);
    }

    private ModularCrawlResult<Link> GenerateCrawlResult(Action<int> onLinksEnqueued, Action<int> onLinksChecked)
    {
        ModularCrawlResult<Link> crawlResult = new ModularCrawlResult<Link>();
        
        crawlResult.OnLinksEnqueued += onLinksEnqueued;
        crawlResult.OnLinksChecked += onLinksChecked;
        
        return crawlResult;
    }
}