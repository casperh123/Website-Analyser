using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.ScheduledAction;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IScheduleService
{
    Task<ScheduledActionDTO> GetById(Guid id);
    Task<ICollection<ScheduledActionDTO>> GetByWebsiteIds(ICollection<Guid> websiteIds);
    Task<ScheduledActionDTO?> GetActionByWebsiteIdAndType(Guid websiteId, CrawlAction type);

    Task<ScheduledActionDTO> ScheduleAction(
        Guid websiteId,
        CrawlAction action,
        Frequency frequency,
        TimeSpan negativeOffset = default);

    Task DeleteAction(Guid actionId);
    Task DeleteTasksByUrlAndUserId(string url, Guid userId);
    Task ResetActionStatus(Guid actionId);

    Task<ICollection<ScheduledActionDTO>> GetDueSchedulesBy(CrawlAction action);

    Task StartAction(Guid actionId);
    Task CompleteAction(Guid actionId);
    Task FailAction(Guid actionId);
}