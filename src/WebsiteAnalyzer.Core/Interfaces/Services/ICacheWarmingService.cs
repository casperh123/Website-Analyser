using Crawl.Models;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.CacheWarm;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface ICacheWarmingService
{
    Task<AnonymousCacheWarm> WarmCacheAnonymous(string url,
        IProgress<CrawlProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task WarmCache(Guid websiteId, IProgress<CrawlProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task WarmCache(Guid websiteId, CancellationToken cancellationToken = default);
    Task<ICollection<CacheWarmDTO>> GetCacheWarmsByWebsiteId(Guid websiteId);
}