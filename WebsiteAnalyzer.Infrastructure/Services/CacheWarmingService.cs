using System.Collections;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface ICacheWarmingService
{
    Task<CacheWarmRun> WarmCacheAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked);
    Task<IEnumerable<CacheWarmRun>> GetCacheWarmRunsAsync();
}

public class CacheWarmingService : ICacheWarmingService
{
    private HttpClient _httpClient;
    private ModularCrawler<Link>? _crawler;
    private ICacheWarmRunRepository _cacheWarmRunRepository;

    public CacheWarmingService(HttpClient httpClient, ICacheWarmRunRepository cacheWarmRunRepository)
    {
        _httpClient = httpClient;
        _cacheWarmRunRepository = cacheWarmRunRepository;
    }

    public async Task<CacheWarmRun> WarmCacheAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked)
    {
        CacheWarmRun cacheWarmRun = new CacheWarmRun()
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now
            
        };
        
        await _cacheWarmRunRepository.AddAsync(cacheWarmRun);
        
        ILinkProcessor<Link> linkProcessor = new LinkProcessor(_httpClient);
        ModularCrawlResult<Link> crawlResult = GenerateCrawlResult(onLinkEnqueued, onLinkChecked);
        ModularCrawler<Link> crawler = new ModularCrawler<Link>(linkProcessor);
            
        await crawler.CrawlWebsiteAsync(new Link(url), crawlResult);

        cacheWarmRun.EndTime = DateTime.Now;
        cacheWarmRun.VisitedPages = crawlResult.LinksChecked;

        await _cacheWarmRunRepository.UpdateAsync(cacheWarmRun);
        
        return cacheWarmRun;
    }

    public async Task<IEnumerable<CacheWarmRun>>GetCacheWarmRunsAsync()
    {
        return await _cacheWarmRunRepository.GetAllAsync();
    }

    private ModularCrawlResult<Link> GenerateCrawlResult(Action<int> onLinksEnqueued, Action<int> onLinksChecked)
    {
        ModularCrawlResult<Link> crawlResult = new ModularCrawlResult<Link>();
        
        crawlResult.OnLinksEnqueued += onLinksEnqueued;
        crawlResult.OnLinksChecked += onLinksChecked;
        
        return crawlResult;
    }
}