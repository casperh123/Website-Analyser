using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IScheduleService
{
    Task<ScheduledAction> GetActionByWebsiteIdAndType(Guid websiteId, CrawlAction type);

    Task<ScheduledAction> ScheduleAction(
        Website website,
        CrawlAction action,
        Frequency frequency,
        TimeSpan negativeOffset = default);
    
    Task DeleteAction(ScheduledAction scheduledTask);
    Task<ICollection<ScheduledAction>> GetScheduledTasksByUserIdAndTypeAsync(Guid? userId, CrawlAction action);
    Task DeleteTasksByUrlAndUserId(string url, Guid userId);

    Task UpdateStatus(ScheduledAction action, Status status);

    Task<ICollection<ScheduledAction>> GetDueSchedulesBy(CrawlAction action);

    Task StartAction(ScheduledAction action);
    Task CompleteAction(ScheduledAction action);
    Task FailAction(ScheduledAction action);
}