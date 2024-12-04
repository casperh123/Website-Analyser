using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.models.Links;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface ICacheWarmingService
{
    Task<CacheWarm> WarmCacheAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked);
    Task<ICollection<CacheWarm>> GetCacheWarmsAsync();
}

public class CacheWarmingService : ICacheWarmingService
{
    private readonly ILinkCrawlerService _crawlerService;
    private readonly IWebsiteService _websiteService;
    private readonly ICacheWarmRepository _cacheWarmRepository;

    public CacheWarmingService(
        ILinkCrawlerService crawlerService,
        ICacheWarmRepository cacheWarmRepository,
        IWebsiteService websiteService)
    {
        _crawlerService = crawlerService;
        _cacheWarmRepository = cacheWarmRepository;
        _websiteService = websiteService;
    }

    public async Task<CacheWarm> WarmCacheAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked)
    {
        Website website = await _websiteService.GetOrAddWebsite(url);
        CacheWarm cacheWarm = await CreateCacheWarmEntry(website);

        ModularCrawlResult<Link> crawlResult =
            await _crawlerService.CrawlWebsiteAsync(url, onLinkEnqueued, onLinkChecked);

        await UpdateCacheWarmResults(cacheWarm, crawlResult);
        return cacheWarm;
    }

    private async Task<CacheWarm> CreateCacheWarmEntry(Website website)
    {
        CacheWarm cacheWarm = new CacheWarm
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now,
            Website = website,
            WebsiteUrl = website.Url,
        };

        await _cacheWarmRepository.AddAsync(cacheWarm);
        return cacheWarm;
    }

    private async Task UpdateCacheWarmResults(CacheWarm cacheWarm, ModularCrawlResult<Link> crawlResult)
    {
        cacheWarm.EndTime = DateTime.Now;
        cacheWarm.VisitedPages = crawlResult.LinksChecked;
        await _cacheWarmRepository.UpdateAsync(cacheWarm);
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsAsync()
    {
        return await _cacheWarmRepository.GetAllAsync();
    }
}