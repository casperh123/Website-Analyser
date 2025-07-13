using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Web.BackgroundJobs.Timers;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public abstract class CrawlBackgroundServiceBase : IHostedService
{
    private readonly IPeriodicTimer _timer;
    private readonly IServiceProvider _serviceProvider;
    private readonly CrawlAction _crawlAction;
    protected readonly ILogger Logger;
    
    private Task _executingTask;

    protected CrawlBackgroundServiceBase(
        ILogger logger,
        IServiceProvider serviceprovider,
        CrawlAction crawlAction)
    {
        _timer = new MinuteTimer();
        Logger = logger;
        _serviceProvider = serviceprovider;
        _crawlAction = crawlAction;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("{ServiceType} background service is starting", _crawlAction);

        // Start the processing loop as a background task and store it
        // This allows the method to return immediately while processing continues
        _executingTask = ProcessingLoop(cancellationToken);

        // Return immediately so other services can start
        return Task.CompletedTask;
    }

    private async Task ProcessingLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && 
               await _timer.WaitForNextTickAsync(cancellationToken))
        {
            await ProcessDueSchedulesAsync(cancellationToken);
        }
    }

    private async Task ProcessDueSchedulesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            ICrawlScheduleRepository repository = scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();
            
            // Get schedules that need processing
            ICollection<CrawlSchedule> schedules = await repository.GetByAction(_crawlAction);
            List<CrawlSchedule> dueSchedules = schedules
                .Where(cs => cs.IsDue() && cs.Action == _crawlAction)
                .ToList();

            Logger.LogInformation("Processing {Count} due {Action} schedules", 
                dueSchedules.Count, _crawlAction);
            
            await Parallel.ForEachAsync(dueSchedules, 
                new ParallelOptions 
                { 
                    MaxDegreeOfParallelism = 3,
                    CancellationToken = cancellationToken 
                }, 
                async (schedule, token) => {
                    using IServiceScope operationScope = _serviceProvider.CreateScope();
                    await ProcessScheduleAsync(schedule, operationScope, token);
                });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing scheduled {Action} tasks", _crawlAction);
        }
    }

    private async ValueTask ProcessScheduleAsync(
        CrawlSchedule schedule, 
        IServiceScope scope, 
        CancellationToken token)
    {
        ICrawlScheduleRepository repository = scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();
        
        try
        {
            schedule.Status = Status.InProgress;
            schedule.LastCrawlDate = DateTime.UtcNow;
            await repository.UpdateAsync(schedule);

            await ExecuteCrawlTaskAsync(schedule, scope, token);

            schedule.Status = Status.Completed;
            await repository.UpdateAsync(schedule);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed {Action} for {Url}", _crawlAction, schedule.Url);
            
            schedule.Status = Status.Failed;
            await repository.UpdateAsync(schedule);
        }
    }

    protected abstract Task ExecuteCrawlTaskAsync(
        CrawlSchedule schedule, 
        IServiceScope scope, 
        CancellationToken token);

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("{ServiceType} background service is stopping", _crawlAction);
        return Task.CompletedTask;
    }
}