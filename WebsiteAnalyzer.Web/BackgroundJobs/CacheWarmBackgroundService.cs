using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class CacheWarmBackgroundService : CrawlBackgroundServiceBase
{
    public CacheWarmBackgroundService(
        IServiceProvider serviceProvider, 
        IPeriodicTimer timer,
        ILogger<CacheWarmBackgroundService> logger) : base(serviceProvider, timer, logger, CrawlAction.CacheWarm)
    {
    }

    protected override async Task ExecuteCrawlAsync(IServiceScope scope, CrawlSchedule schedule)
    {
        ICrawlScheduleRepository crawlScheduleRepository =
            scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();
        ICacheWarmingService cacheWarmingService = 
            scope.ServiceProvider.GetRequiredService<ICacheWarmingService>();

        _logger.LogInformation(
            "Starting cache warm for URL " +
            "{Url} (Schedule ID: {ScheduleId})", 
            schedule.Url, 
            schedule.UserId);

        schedule.Status = Status.InProgress;
        schedule.LastCrawlDate = DateTime.UtcNow;

        await crawlScheduleRepository.UpdateAsync(schedule);

        try
        {
            await cacheWarmingService.WarmCacheWithoutMetrics(schedule.Url, schedule.UserId);
            
            schedule.Status = Status.Completed;
            await crawlScheduleRepository.UpdateAsync(schedule);

            // Log successful completion    
            _logger.LogInformation(
                "Successfully completed cache warm for URL {Url} (Schedule ID: {ScheduleId})", 
                schedule.Url, 
                schedule.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to warm cache for URL {Url} (Schedule ID: {ScheduleId})", 
                schedule.Url, 
                schedule.UserId);

            schedule.Status = Status.Failed;
            await crawlScheduleRepository.UpdateAsync(schedule);
        }    }
}