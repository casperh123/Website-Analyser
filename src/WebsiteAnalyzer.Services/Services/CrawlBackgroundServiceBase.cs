using System.Threading.Tasks.Dataflow;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Web.BackgroundJobs.Timers;

namespace WebsiteAnalyzer.Services.Services;

public abstract class CrawlBackgroundServiceBase : BackgroundService
{
    private readonly IPeriodicTimer _timer;
    private readonly IServiceProvider _serviceProvider;
    private readonly CrawlAction _crawlAction;
    private readonly ActionBlock<ScheduledAction> _crawlProcessor;
    
    protected readonly ILogger Logger;

    protected CrawlBackgroundServiceBase(
        ILogger logger,
        IServiceProvider serviceprovider,
        CrawlAction crawlAction)
    {
        _timer = new MinuteTimer(1);
        Logger = logger;
        _serviceProvider = serviceprovider;
        _crawlAction = crawlAction;

        _crawlProcessor = new ActionBlock<ScheduledAction>(async scheduledAction =>
            await ProcessScheduleWithTimeoutAsync(scheduledAction),
            
            new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = 10,
                EnsureOrdered = false,                
            });
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(stoppingToken))
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            IScheduleService scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
        
            ICollection<ScheduledAction> dueScheduledActions = await scheduleService.GetDueSchedulesBy(_crawlAction);

            Logger.LogInformation("Processing {Count} due {Action} schedules", 
                dueScheduledActions.Count, _crawlAction);

            foreach (ScheduledAction action in dueScheduledActions)
            {
                await _crawlProcessor.SendAsync(action, stoppingToken);
            }
        }
    }
    
    private async Task ProcessScheduleWithTimeoutAsync(ScheduledAction scheduledAction)
    {
        using CancellationTokenSource timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(120));
        using IServiceScope scope = _serviceProvider.CreateScope();
    
        try
        {
            await ProcessScheduleAsync(scheduledAction, scope, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            Logger.LogWarning("Crawl task timed out after 5 minutes for {Url}", 
                scheduledAction.Website.Url);
        }
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

            await ExecuteTaskAsync(scheduledAction, scope, token);

            await scheduleService.CompleteAction(scheduledAction);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed {Action} for {Url}", _crawlAction, scheduledAction.Website.Url);

            await scheduleService.FailAction(scheduledAction);
        }
    }

    protected abstract Task ExecuteTaskAsync(
        ScheduledAction schedule,
        IServiceScope scope, 
        CancellationToken token);
}
