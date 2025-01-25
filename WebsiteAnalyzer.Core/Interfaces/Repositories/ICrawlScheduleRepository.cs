using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.Core.Persistence;

public interface ICrawlScheduleRepository : IBaseRepository<CrawlSchedule>
{
    Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAsync(Guid userId);
    Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAndTypeAsync(Guid userId, CrawlAction action);
}