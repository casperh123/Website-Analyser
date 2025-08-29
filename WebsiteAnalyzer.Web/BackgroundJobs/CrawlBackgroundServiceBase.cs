using WebsiteAnalyzer.Core.Domain;
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
                    MaxDegreeOfParallelism = 10,
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
        ScheduledAction scheduledAction, 
        IServiceScope scope, 
        CancellationToken token)
    {
        IScheduledActionRepository repository = scope.ServiceProvider.GetRequiredService<IScheduledActionRepository>();

        try
        {
            scheduledAction.StartAction();
            await repository.UpdateAsync(scheduledAction);

            await ExecuteCrawlTaskAsync(scheduledAction, scope, token);

            scheduledAction.Status = Status.Completed;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed {Action} for {Url}", _crawlAction, scheduledAction.Website.Url);

            scheduledAction.Status = Status.Failed;
        }
        finally
        {
            await repository.UpdateAsync(scheduledAction);
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