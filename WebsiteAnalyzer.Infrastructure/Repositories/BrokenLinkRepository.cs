using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class BrokenLinkRepository : BaseRepository<BrokenLink>, IBrokenLinkRepository
{
    public BrokenLinkRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(dbContextFactory)
    {
        
    }

    public async Task<IEnumerable<BrokenLink>> GetBrokenLinksByCrawlAsync(Guid brokenLinkCrawlId)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.BrokenLinks
            .Where(bl => bl.BrokenLinkCrawlId == brokenLinkCrawlId)
            .ToListAsync();
    }
}