using WebsiteAnalyzer.Core.Entities.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IBrokenLinkCrawlRepository : IBaseRepository<BrokenLinkCrawl>
{
    Task<ICollection<BrokenLinkCrawl>?> GetByUserAsync(Guid? userId);
    Task<BrokenLinkCrawl> GetByIdUrlUserId(Guid id, string url, Guid userId);
}