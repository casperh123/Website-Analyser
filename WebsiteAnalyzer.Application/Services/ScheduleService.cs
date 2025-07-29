using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Exceptions;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using ICacheWarmingService = WebsiteAnalyzer.Core.Interfaces.Services.ICacheWarmingService;

namespace WebsiteAnalyzer.Application.Services;


public class ScheduleService : IScheduleService
{
    private readonly IScheduledActionRepository _scheduleRepository;
    private readonly HttpClient _httpClient;

    public ScheduleService(
        IScheduledActionRepository scheduleRepository,
        HttpClient httpClient)
    {
        _scheduleRepository = scheduleRepository;
        _httpClient = httpClient;
    }

    public async Task<ScheduledAction> ScheduleTask(Website website, CrawlAction action, Frequency frequency, DateTime firstCrawl)
    {
        ScheduledAction scheduledAction = new ScheduledAction(
            website,
            frequency,
            action,
            firstCrawl
        );

        await _scheduleRepository.AddAsync(scheduledAction);

        return scheduledAction;
    }

    public async Task UpdateScheduledTask(ScheduledAction scheduledTask, Frequency frequency)
    {
        scheduledTask.Frequency = frequency;

        await _scheduleRepository.UpdateAsync(scheduledTask);
    }

    public async Task DeleteScheduledTask(ScheduledAction scheduledTask)
    {
        await _scheduleRepository.DeleteAsync(scheduledTask).ConfigureAwait(false);
    }

    public async Task<ICollection<ScheduledAction>> GetScheduledTasksByUserIdAndTypeAsync(Guid? userId, CrawlAction action)
    {
        if (!userId.HasValue)
        {
            return [];
        }
        
        return await _scheduleRepository.GetCrawlSchedulesByUserIdAndTypeAsync(userId.Value, action);
    }

    public async Task DeleteTasksByUrlAndUserId(string url, Guid userId)
    {
        await _scheduleRepository.DeleteByUrlAndUserId(url, userId);
    }
}