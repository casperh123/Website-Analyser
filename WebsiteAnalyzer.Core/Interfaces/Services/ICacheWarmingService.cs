using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Events;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface ICacheWarmingService
{
    event EventHandler<CrawlProgressEventArgs> ProgressUpdated;
    
    Task WarmCache(string url, CancellationToken cancellationToken = default);
    Task WarmCacheWithoutMetrics(Website website, CancellationToken cancellationToken = default);
    Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid? userId);

    Task<ICollection<CacheWarm>> GetCacheWarmsByWebsiteId(Guid websiteId);
}