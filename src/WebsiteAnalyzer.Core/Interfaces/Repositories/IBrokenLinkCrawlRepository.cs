using WebsiteAnalyzer.Core.Domain.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IBrokenLinkCrawlRepository : IBaseRepository<BrokenLinkCrawl>
{
    Task<ICollection<BrokenLinkCrawl>?> GetByUserAsync(Guid? userId);
    Task<ICollection<BrokenLinkCrawl>> GetByUrlUserId(string url, Guid userId);
}