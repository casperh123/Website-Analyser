using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class BrokenLinkBackgroundService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPeriodicTimer _timer;
    private readonly ILogger<BrokenLinkBackgroundService> _logger;

    public BrokenLinkBackgroundService(
        IServiceProvider serviceProvider, 
        IPeriodicTimer timer,
        ILogger<BrokenLinkBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _timer = timer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Log when the service starts - this helps track service lifecycle
        _logger.LogInformation("Cache warming background service is starting");

        using IServiceScope scope = _serviceProvider.CreateScope();
        ICrawlScheduleRepository crawlScheduleRepository =
            scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();

        while (!cancellationToken.IsCancellationRequested
               && await _timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                ICollection<CrawlSchedule> scheduledItems = await crawlScheduleRepository.GetAllAsync();
                ICollection<CrawlSchedule> dueSchedules = scheduledItems.Where(IsDue).ToList();
                
                _logger.LogInformation("Found {TotalSchedules} schedules, {DueSchedules} are due for processing", scheduledItems.Count, dueSchedules.Count());

                int maxDegreeOfParallelism = 5;

                await Parallel.ForEachAsync(dueSchedules, new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                }, async (crawlSchedule, token) => 
                { 
                    await WarmCacheAsync(crawlSchedule);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in cache warming background service");
            }
        }
    }

    private async Task WarmCacheAsync(CrawlSchedule crawlSchedule)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        ICrawlScheduleRepository crawlScheduleRepository =
            scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();
        ICacheWarmingService cacheWarmingService = 
            scope.ServiceProvider.GetRequiredService<ICacheWarmingService>();

        _logger.LogInformation(
            "Starting cache warm for URL {Url} (Schedule ID: {ScheduleId})", 
            crawlSchedule.Url, 
            crawlSchedule.UserId);

        crawlSchedule.Status = Status.InProgress;
        crawlSchedule.LastCrawlDate = DateTime.UtcNow;

        await crawlScheduleRepository.UpdateAsync(crawlSchedule);

        try
        {
            await cacheWarmingService.WarmCacheWithoutMetrics(crawlSchedule.Url, crawlSchedule.UserId);
            
            crawlSchedule.Status = Status.Completed;
            await crawlScheduleRepository.UpdateAsync(crawlSchedule);

            // Log successful completion
            _logger.LogInformation(
                "Successfully completed cache warm for URL {Url} (Schedule ID: {ScheduleId})", 
                crawlSchedule.Url, 
                crawlSchedule.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to warm cache for URL {Url} (Schedule ID: {ScheduleId})", 
                crawlSchedule.Url, 
                crawlSchedule.UserId);

            crawlSchedule.Status = Status.Failed;
            await crawlScheduleRepository.UpdateAsync(crawlSchedule);
        }
    }

    private bool IsDue(CrawlSchedule crawlSchedule)
    {
        if (crawlSchedule.Status is Status.InProgress)
        {
            return false;
        }

        DateTime currentDate = DateTime.UtcNow;

        return crawlSchedule.Frequency switch
        {
            Frequency.SixHourly => crawlSchedule.LastCrawlDate.AddHours(6) <= currentDate,
            Frequency.TwelveHourly => crawlSchedule.LastCrawlDate.AddHours(12) <= currentDate,
            Frequency.Daily => crawlSchedule.LastCrawlDate.AddDays(1) <= currentDate,
            Frequency.Weekly => crawlSchedule.LastCrawlDate.AddDays(7) <= currentDate,
            _ => false
        };
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache warming background service is stopping");
        return Task.CompletedTask;
    }
}