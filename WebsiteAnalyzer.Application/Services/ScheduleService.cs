using WebsiteAnalyzer.Core.Domain;
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

    public ScheduleService(
        IScheduledActionRepository scheduleRepository 
        )
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<ScheduledAction> ScheduleAction(Website website, CrawlAction action, Frequency frequency)
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

    public async Task<ScheduledAction> GetActionByWebsiteId(Guid websiteId)
    {
        return await _scheduleRepository.GetActionByWebsiteId(websiteId);
    }

    public async Task UpdateActionFrequency(ScheduledAction scheduledTask, Frequency frequency)
    {
        scheduledTask.Frequency = frequency;

        await _scheduleRepository.UpdateAsync(scheduledTask);
    }

    public async Task DeleteAction(ScheduledAction scheduledTask)
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