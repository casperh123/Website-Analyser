using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class BrokenLinkRepository : BaseRepository<BrokenLink>, IBrokenLinkRepository
{
    public BrokenLinkRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }

    public async Task<IEnumerable<BrokenLink>> GetBrokenLinksByCrawlAsync(Guid brokenLinkCrawlId)
    {
        return await DbSet
            .Where(bl => bl.BrokenLinkCrawlId == brokenLinkCrawlId)
            .ToListAsync();
    }
}