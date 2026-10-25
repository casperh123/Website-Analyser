using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IBrokenLinkCrawlRepository : IBaseRepository<BrokenLinkCrawl>
{
    Task<ICollection<BrokenLinkCrawl>> GetByWebsiteId(Guid websiteId);
    Task<PagedResponse<BrokenLinkCrawl>> GetByWebsiteId(Guid websiteId, int page = 1, int perPage = 20);
}
