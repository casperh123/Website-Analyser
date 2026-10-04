using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class BrokenLinkCrawlRepository : BaseRepository<BrokenLinkCrawl>, IBrokenLinkCrawlRepository
{
    public BrokenLinkCrawlRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }

    public async Task<ICollection<BrokenLinkCrawl>> GetByUserAsync(Guid userId)
    {
        return await DbContext.BrokenLinkCrawls
            .Where(crawl => crawl.UserId == userId)
            .Include(crawl => crawl.BrokenLinks)
            .ToListAsync();
    }

   public async Task<ICollection<BrokenLinkCrawl>> GetByUrlUserId(string url, Guid userId)
    {
        return await DbContext.BrokenLinkCrawls
            .Where(crawl => crawl.Url == url)
            .Where(crawl => crawl.UserId == userId)
            .Include(crawl => crawl.BrokenLinks)
            .OrderByDescending(crawl => crawl.Date)
            .ToListAsync();
    }
}
