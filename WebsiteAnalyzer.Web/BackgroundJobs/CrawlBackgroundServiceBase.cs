using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
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
        _timer = new MinuteTimer(10);
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
        using IServiceScope scope = _serviceProvider.CreateScope();
        IScheduledActionRepository repository = scope.ServiceProvider.GetRequiredService<IScheduledActionRepository>();
        
        // Get schedules that need processing
        ICollection<ScheduledAction> schedules = await repository.GetByAction(_crawlAction);
        List<ScheduledAction> dueSchedules = schedules
            .Where(cs => cs.IsDueForExecution && cs.Action == _crawlAction)
            .ToList();

        Logger.LogInformation("Processing {Count} due {Action} schedules", 
            dueSchedules.Count, _crawlAction);
        
        await Parallel.ForEachAsync(dueSchedules, 
            new ParallelOptions 
            { 
                CancellationToken = cancellationToken, 
            }, 
            async (schedule, token) => {
                using IServiceScope operationScope = _serviceProvider.CreateScope();
                await ProcessScheduleAsync(schedule, operationScope, token);
            });
        
    }

    private async ValueTask ProcessScheduleAsync(
        ScheduledAction scheduledAction, 
        IServiceScope scope, 
        CancellationToken token)
    {
        IScheduleService scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();

        try
        {
            await scheduleService.StartAction(scheduledAction);

            await ExecuteCrawlTaskAsync(scheduledAction, scope, token);

            await scheduleService.CompleteAction(scheduledAction);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed {Action} for {Url}", _crawlAction, scheduledAction.Website.Url);

            await scheduleService.FailAction(scheduledAction);
        }
    }

    protected abstract Task ExecuteCrawlTaskAsync(
        ScheduledAction schedule, 
        IServiceScope scope, 
        CancellationToken token);

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("{ServiceType} background service is stopping", _crawlAction);
        return Task.CompletedTask;
    }
}
