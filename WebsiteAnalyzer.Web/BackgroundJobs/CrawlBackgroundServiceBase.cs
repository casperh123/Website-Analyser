using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public abstract class CrawlBackgroundServiceBase : IHostedService
{
    private readonly IPeriodicTimer _timer;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ILogger _logger;
    protected readonly CrawlAction _crawlAction;

    protected CrawlBackgroundServiceBase(
        IServiceProvider serviceProvider,
        IPeriodicTimer timer,
        ILogger logger,
        CrawlAction crawlAction)
    {
        _serviceProvider = serviceProvider;
        _timer = timer;
        _logger = logger;
        _crawlAction = crawlAction;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceType} background service is starting", _crawlAction);

        while (!cancellationToken.IsCancellationRequested && 
               await _timer.WaitForNextTickAsync(cancellationToken))
        {
            await ProcessDueSchedulesAsync();
        }
    }

    private async Task ProcessDueSchedulesAsync()
    {
        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            ICrawlScheduleRepository repository = scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();

            ICollection<CrawlSchedule> schedules = await repository.GetAllAsync();
            List<CrawlSchedule> dueSchedules = schedules
                .Where(cs => cs.IsDue() && cs.Action == _crawlAction)
                .ToList();

            _logger.LogInformation("Processing {Count} due {Action} schedules", 
                dueSchedules.Count, _crawlAction);

            await Parallel.ForEachAsync(dueSchedules, 
                new ParallelOptions { MaxDegreeOfParallelism = 5 }, 
                ProcessScheduleAsync);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled {Action} tasks", _crawlAction);
        }
    }

    private async ValueTask ProcessScheduleAsync(CrawlSchedule schedule, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();

        try
        {
            await UpdateScheduleStatusAsync(repository, schedule, Status.InProgress);
            
            // This is where specific crawl implementations will differ
            await ExecuteCrawlAsync(scope, schedule);
            
            await UpdateScheduleStatusAsync(repository, schedule, Status.Completed);
            
            _logger.LogInformation("Completed {Action} for {Url}", _crawlAction, schedule.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed {Action} for {Url}", _crawlAction, schedule.Url);
            await UpdateScheduleStatusAsync(repository, schedule, Status.Failed);
        }
    }

    protected abstract Task ExecuteCrawlAsync(IServiceScope scope, CrawlSchedule schedule);

    private async Task UpdateScheduleStatusAsync(
        ICrawlScheduleRepository repository, 
        CrawlSchedule schedule, 
        Status status)
    {
        schedule.Status = status;
        if (status == Status.InProgress)
        {
            schedule.LastCrawlDate = DateTime.UtcNow;
        }
        await repository.UpdateAsync(schedule);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceType} background service is stopping", _crawlAction);
        return Task.CompletedTask;
    }
}