using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

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

    public async Task<ScheduledAction> ScheduleAction(
        Website website, 
        CrawlAction action, 
        Frequency frequency, 
        TimeSpan negativeOffset = default)
    {
        ScheduledAction scheduledAction = new ScheduledAction(
            website,
            frequency,
            action,
            negativeOffset
        );

        await _scheduleRepository.AddAsync(scheduledAction);

        return scheduledAction;
    }

    public async Task<ScheduledAction> GetActionByWebsiteIdAndType(Guid websiteId, CrawlAction type)
    {
        return await _scheduleRepository.GetByWebsiteIdAndType(websiteId, type);
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