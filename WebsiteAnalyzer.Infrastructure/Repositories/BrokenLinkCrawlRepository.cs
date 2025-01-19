using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class BrokenLinkCrawlRepository : BaseRepository<BrokenLinkCrawl>, IBrokenLinkCrawlRepository
{
    public BrokenLinkCrawlRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }
}