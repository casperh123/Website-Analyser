using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class CacheWarmBackgroundService : CrawlBackgroundServiceBase
{
    public CacheWarmBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CacheWarmBackgroundService> logger) : base(logger, serviceProvider, CrawlAction.CacheWarm)
    {
    }

    protected override async Task ExecuteCrawlTaskAsync(
        CrawlSchedule schedule,
        IServiceScope scope,
        CancellationToken token)
    {
        ICacheWarmingService cacheWarmingService = scope.ServiceProvider.GetService<ICacheWarmingService>()!;
        await cacheWarmingService.WarmCacheWithoutMetrics(schedule.Url, schedule.UserId, token);
    }
}