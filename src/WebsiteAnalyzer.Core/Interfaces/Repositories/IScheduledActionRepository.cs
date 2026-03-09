using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IScheduledActionRepository : IBaseRepository<ScheduledAction>
{
    Task<ScheduledAction?> GetByWebsiteIdAndType(Guid websiteId, CrawlAction type);
    Task<ICollection<ScheduledAction>> GetCrawlSchedulesByUserIdAndTypeAsync(Guid userId, CrawlAction action);
    Task<ICollection<ScheduledAction>> GetByAction(CrawlAction action);
    Task<ICollection<ScheduledAction>> GetByWebsiteId(Guid websiteId);
    Task<ICollection<ScheduledAction>> GetByWebsiteIds(ICollection<Guid> websiteIds);
    Task DeleteByUrlAndUserId(string url, Guid userId);
}