using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Entities.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IBrokenLinkRepository : IBaseRepository<BrokenLink>
{
    public Task<IEnumerable<BrokenLink>> GetBrokenLinksByCrawlAsync(Guid brokenLinkCrawlId);
}