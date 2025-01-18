using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface ICacheWarmingService
{
    Task<CacheWarm> WarmCacheWithSaveAsync(
        string url,
        Guid userId,
        Action<int> onLinkEnqueued,
        Action<int> onLinkChecked,
        CancellationToken cancellationToken = default
    );

    Task WarmCache(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked,
        CancellationToken cancellationToken = default);

    Task WarmCacheWithoutMetrics(string url, Guid userId, CancellationToken cancellationToken = default);
    Task<ICollection<CacheWarm>> GetCacheWarmsAsync();
    Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid userId);
}