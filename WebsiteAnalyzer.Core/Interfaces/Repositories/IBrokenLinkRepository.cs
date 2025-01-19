using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IBrokenLinkRepository : IBaseRepository<BrokenLink>
{
    public Task<IEnumerable<BrokenLink>> GetBrokenLinksByCrawlAsync(Guid brokenLinkCrawlId);
}