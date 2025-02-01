using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface ICrawlScheduleRepository : IBaseRepository<CrawlSchedule>
{
    Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAsync(Guid userId);
    Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAndTypeAsync(Guid userId, CrawlAction action);
    Task<CrawlSchedule?> GetCrawlScheduleBy(string url, Guid userId, CrawlAction action);
}