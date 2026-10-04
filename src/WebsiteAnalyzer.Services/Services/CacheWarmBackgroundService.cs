using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.ScheduledAction;

namespace WebsiteAnalyzer.Services.Services;

public class CacheWarmBackgroundService : CrawlBackgroundServiceBase
{
    public CacheWarmBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CacheWarmBackgroundService> logger) : base(logger, serviceProvider, CrawlAction.CacheWarm)
    {
    }

    protected override async Task ExecuteTaskAsync(
        ScheduledActionDTO action,
        IServiceScope scope,
        CancellationToken token)
    {
        ICacheWarmingService cacheWarmingService = scope.ServiceProvider.GetService<ICacheWarmingService>()!;
        await cacheWarmingService.WarmCache(action.Website.Id, token);
    }
}
