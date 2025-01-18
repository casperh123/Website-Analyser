using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Persistence;
using ICacheWarmingService = WebsiteAnalyzer.Core.Interfaces.Services.ICacheWarmingService;

namespace WebsiteAnalyzer.Application.Services;


public class ScheduleService : IScheduleService
{
    private readonly ICrawlScheduleRepository _scheduleRepository;
    private readonly ICacheWarmingService _cacheWarmingService;
    private readonly IBrokenLinkService _brokenLinkService;

    public ScheduleService(
        ICrawlScheduleRepository scheduleRepository,
        ICacheWarmingService cacheWarmingService,
        IBrokenLinkService brokenLinkService)
    {
        _scheduleRepository = scheduleRepository;
        _cacheWarmingService = cacheWarmingService;
        _brokenLinkService = brokenLinkService;
    }

    public async Task<CrawlSchedule> ScheduleTask(string url, Guid userId, CrawlAction action, Frequency frequency)
    {
        CrawlSchedule scheduledAction = new CrawlSchedule()
        {
            UserId = userId,
            WebsiteUrl = url,
            Frequency = frequency,
            Action = action,
            LastCrawlDate = DateTime.Today.Subtract(TimeSpan.FromDays(14)),
            Status = Status.Scheduled
        };

        await _scheduleRepository.AddAsync(scheduledAction).ConfigureAwait(false);

        return scheduledAction;
    }

    public async Task UpdateScheduledTask(CrawlSchedule scheduledTask, Frequency frequency)
    {
        scheduledTask.Frequency = frequency;

        await _scheduleRepository.UpdateAsync(scheduledTask);
    }

    public async Task DeleteScheduledTask(CrawlSchedule scheduledTask)
    {
        await _scheduleRepository.DeleteAsync(scheduledTask).ConfigureAwait(false);
    }

    public async Task RunScheduledTask(CrawlSchedule schedule)
    {
        switch (schedule.Action)
        {
            case CrawlAction.BrokenLink:
                //TODO IMPLEMENT
                break;
            case CrawlAction.CacheWarm:
                await _cacheWarmingService.WarmCacheWithoutMetrics(schedule.WebsiteUrl, schedule.UserId);
                break;
        }
    }

    public async Task<ICollection<CrawlSchedule>> GetScheduledTasksByUserIdAndTypeAsync(Guid userId, CrawlAction action)
    {
        return await _scheduleRepository.GetCrawlSchedulesByUserIdAndTypeAsync(userId, action);
    }
}