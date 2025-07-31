using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IScheduledActionRepository : IBaseRepository<ScheduledAction>
{
    Task<ICollection<ScheduledAction>> GetCrawlSchedulesByUserIdAndTypeAsync(Guid userId, CrawlAction action);
    Task<ICollection<ScheduledAction>> GetByAction(CrawlAction action);
    Task DeleteByUrlAndUserId(string url, Guid userId);
    Task<ScheduledAction> GetActionByWebsiteId(Guid websiteId);
}