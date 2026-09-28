using Crawl.Core;
using Crawl.Core.Builders;
using Crawl.Filters;
using Crawl.Models;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.CacheWarm;

namespace WebsiteAnalyzer.Application.Services;

public class CacheWarmingService : ICacheWarmingService
{
    private readonly ICacheWarmRepository _cacheWarmRepository;
    private readonly IWebsiteRepository _websiteRepository;
    private readonly HttpClient _httpClient;
    
    public CacheWarmingService(
        ICacheWarmRepository cacheWarmRepository,
        IWebsiteRepository websiteRepository,
        HttpClient httpClient
    )
    {
        _cacheWarmRepository = cacheWarmRepository;
        _websiteRepository = websiteRepository;
        _httpClient = httpClient;
    }

    public async Task<AnonymousCacheWarm> WarmCacheAnonymous(
        string url, 
        IProgress<CrawlProgress>? progress = null, 
        CancellationToken cancellationToken = default
        )
    {
        CrawlTimer timer = new CrawlTimer();
        int linksChecked = await CrawlWebsiteCore(url, progress, cancellationToken);
        CrawlTimerResult time = timer.Complete();
        
        return new AnonymousCacheWarm(linksChecked, time.StartTime, time.EndTime);
    }

    public async Task WarmCache(
        Guid websiteId,
        CancellationToken cancellationToken = default
    )
    {
        await WarmCache(websiteId, null, cancellationToken);
    }

    public async Task WarmCache(
        Guid websiteId, 
        IProgress<CrawlProgress>? progress = null, 
        CancellationToken cancellationToken = default
        )
    {
        Website website = await _websiteRepository.GetByWebsiteId(websiteId);
        CrawlTimer timer = new CrawlTimer();
        int linksChecked = await CrawlWebsiteCore(website.Url, progress, cancellationToken);
        CrawlTimerResult time = timer.Complete();
        CacheWarm cacheWarm = new CacheWarm(website, linksChecked, time.StartTime, time.EndTime);

        await _cacheWarmRepository.AddAsync(cacheWarm);
    }

    public async Task<ICollection<CacheWarmDTO>> GetCacheWarmsByWebsiteId(Guid websiteId)
    {
        ICollection<CacheWarm> cacheWarms = await _cacheWarmRepository.GetByWebsiteId(websiteId);
        
        return [.. cacheWarms.Select(CacheWarmDTO.From)];
    }
    
    private async Task<int> CrawlWebsiteCore(
        string url, 
        IProgress<CrawlProgress>? progress, 
        CancellationToken cancellationToken)
    {
        int linksChecked = 0;

        Progress<CrawlProgress> trackingProgress = new Progress<CrawlProgress>(p =>
        {
            linksChecked = p.TotalCrawled;
            progress?.Report(p);
        });

        Crawler crawler = new SequentialCrawlerBuilder(_httpClient)
            .WithFilter(new SameHostFilter())
            .Build();

        await crawler.CrawlAsync(new Uri(url), trackingProgress, cancellationToken);
    
        return linksChecked;
    }
}