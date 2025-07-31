using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IScheduleService
{
    Task<ScheduledAction> GetActionByWebsiteId(Guid websiteId);
    Task<ScheduledAction> ScheduleAction(Website website, CrawlAction action, Frequency frequency, DateTime firstCrawl);
    Task DeleteAction(ScheduledAction scheduledTask);
    Task<ICollection<ScheduledAction>> GetScheduledTasksByUserIdAndTypeAsync(Guid? userId, CrawlAction action);
    Task DeleteTasksByUrlAndUserId(string url, Guid userId);
}