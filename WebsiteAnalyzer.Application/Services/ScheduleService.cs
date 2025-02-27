using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Exceptions;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using ICacheWarmingService = WebsiteAnalyzer.Core.Interfaces.Services.ICacheWarmingService;

namespace WebsiteAnalyzer.Application.Services;


public class ScheduleService : IScheduleService
{
    private readonly ICrawlScheduleRepository _scheduleRepository;
    private readonly ICacheWarmingService _cacheWarmingService;
    private readonly IBrokenLinkService _brokenLinkService;
    private readonly HttpClient _httpClient;

    public ScheduleService(
        ICrawlScheduleRepository scheduleRepository,
        ICacheWarmingService cacheWarmingService,
        IBrokenLinkService brokenLinkService,
        HttpClient httpClient)
    {
        _scheduleRepository = scheduleRepository;
        _cacheWarmingService = cacheWarmingService;
        _brokenLinkService = brokenLinkService;
        _httpClient = httpClient;
    }

    public async Task<CrawlSchedule> ScheduleTask(string url, Guid userId, CrawlAction action, Frequency frequency)
    {
        CrawlSchedule scheduledAction = new CrawlSchedule()
        {
            UserId = userId,
            Url = url,
            Frequency = frequency,
            Action = action,
            LastCrawlDate = DateTime.Today.Subtract(TimeSpan.FromDays(14)),
            Status = Status.Scheduled
        };

        await VerifyCrawl(url, userId, scheduledAction.Action);
        await _scheduleRepository.AddAsync(scheduledAction);

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
                await _cacheWarmingService.WarmCacheWithoutMetrics(schedule.Url, schedule.UserId);
                break;
        }
    }

    public async Task<ICollection<CrawlSchedule>> GetScheduledTasksByUserIdAndTypeAsync(Guid? userId, CrawlAction action)
    {
        if (!userId.HasValue)
        {
            return [];
        }
        
        return await _scheduleRepository.GetCrawlSchedulesByUserIdAndTypeAsync(userId.Value, action);
    }

    private async Task VerifyCrawl(string url, Guid userId, CrawlAction action)
    {
        CrawlSchedule? crawlSchedule = await _scheduleRepository.GetCrawlScheduleBy(url, userId, action);

        if (crawlSchedule is not null)
        {
            throw new AlreadyScheduledException($"{url} is already scheduled.");
        }
        
        try
        {
            await _httpClient.GetAsync(url);
        }
        catch (Exception e)
        {
            throw new UrlException($"Could not verify URL: {url}");
        }
    }
}