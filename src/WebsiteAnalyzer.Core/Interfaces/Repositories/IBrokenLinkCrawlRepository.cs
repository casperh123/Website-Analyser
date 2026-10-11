using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IBrokenLinkCrawlRepository : IBaseRepository<BrokenLinkCrawl>
{
    Task<ICollection<BrokenLinkCrawl>> GetByUserAsync(Guid userId);
    Task<PagedResponse<BrokenLinkCrawl>> GetByUserAsync(Guid userId, int page = 1, int perPage = 20);
    Task<ICollection<BrokenLinkCrawl>> GetByUrlUserId(string url, Guid userId);
}
