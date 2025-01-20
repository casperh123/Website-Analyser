using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Events;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface ICacheWarmingService
{
    event EventHandler<CrawlProgressEventArgs> ProgressUpdated;
    
    Task<CacheWarm> WarmCacheWithSaveAsync(string url, Guid userId, CancellationToken cancellationToken = default);
    Task WarmCache(string url, CancellationToken cancellationToken = default);
    Task WarmCacheWithoutMetrics(string url, Guid userId, CancellationToken cancellationToken = default);
    Task<ICollection<CacheWarm>> GetCacheWarmsAsync();
    Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid userId);
}