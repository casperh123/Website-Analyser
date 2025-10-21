using System.Threading.Tasks.Dataflow;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Web.BackgroundJobs.Timers;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public abstract class CrawlBackgroundServiceBase : IHostedService
{
    private readonly IPeriodicTimer _timer;
    private readonly IServiceProvider _serviceProvider;
    private readonly CrawlAction _crawlAction;
    private readonly ActionBlock<ScheduledAction> _crawlProcessor;
    
    protected readonly ILogger Logger;

    private CancellationToken _cancellationToken;
    private Task _executingTask;

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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("{ServiceType} background service is starting", _crawlAction);

        _cancellationToken = cancellationToken;
        
        // Start the processing loop as a background task and store it
        // This allows the method to return immediately while processing continues
        _executingTask = ProcessingLoop();

        // Return immediately so other services can start
        return Task.CompletedTask;
    }

    private async Task ProcessingLoop()
    {
        while (!_cancellationToken.IsCancellationRequested && 
               await _timer.WaitForNextTickAsync(_cancellationToken))
        {
            await ProcessDueSchedulesAsync();
        }
        
        _crawlProcessor.Complete();
        await _crawlProcessor.Completion;
    }

    private async Task ProcessDueSchedulesAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IScheduleService scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
        
        ICollection<ScheduledAction> dueScheduledActions = await scheduleService.GetDueSchedulesBy(_crawlAction);

        Logger.LogInformation("Processing {Count} due {Action} schedules", 
            dueScheduledActions.Count, _crawlAction);

        foreach (ScheduledAction action in dueScheduledActions)
        {
            await _crawlProcessor.SendAsync(action, _cancellationToken);
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

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("{ServiceType} background service is stopping", _crawlAction);
    
        _crawlProcessor.Complete();
        await _crawlProcessor.Completion;
    }
}
