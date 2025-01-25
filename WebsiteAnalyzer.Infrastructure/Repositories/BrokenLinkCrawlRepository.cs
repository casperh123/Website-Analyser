using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class BrokenLinkCrawlRepository : BaseRepository<BrokenLinkCrawl>, IBrokenLinkCrawlRepository
{
    public BrokenLinkCrawlRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }

    public async Task<ICollection<BrokenLinkCrawl>?> GetByUserAsync(Guid? userId)
    {
        return await _dbContext.BrokenLinkCrawls
            .Where(crawl => crawl.UserId == userId)
            .Include(crawl => crawl.BrokenLinks)
            .ToListAsync();
    }
}