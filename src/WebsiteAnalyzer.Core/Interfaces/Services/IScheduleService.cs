using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IScheduleService
{
    Task<ScheduledAction> GetById(Guid id);
    Task<ICollection<ScheduledAction>> GetByWebsiteId(Guid websiteId);
    Task<ICollection<ScheduledAction>> GetByWebsiteIds(ICollection<Guid> websiteIds);
    Task<ScheduledAction?> GetActionByWebsiteIdAndType(Guid websiteId, CrawlAction type);

    Task<ScheduledAction> ScheduleAction(
        Website website,
        CrawlAction action,
        Frequency frequency,
        TimeSpan negativeOffset = default);

    Task DeleteAction(ScheduledAction scheduledTask);
    Task DeleteTasksByUrlAndUserId(string url, Guid userId);
    Task ResetActionStatus(ScheduledAction action);

    Task<ICollection<ScheduledAction>> GetDueSchedulesBy(CrawlAction action);

    Task StartAction(ScheduledAction action);
    Task CompleteAction(ScheduledAction action);
    Task FailAction(ScheduledAction action);
}