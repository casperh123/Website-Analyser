using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class BrokenLinkCrawlRepository : BaseRepository<BrokenLinkCrawl>, IBrokenLinkCrawlRepository
{
    public BrokenLinkCrawlRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(dbContextFactory)
    {
        
    }

    public async Task<ICollection<BrokenLinkCrawl>?> GetByUserAsync(Guid? userId)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.BrokenLinkCrawls
            .Where(crawl => crawl.UserId == userId)
            .Include(crawl => crawl.BrokenLinks)
            .ToListAsync();
    }

    public async Task<BrokenLinkCrawl> GetByIdUrlUserId(Guid id, string url, Guid userId)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.BrokenLinkCrawls
            .Where(crawl => crawl.Id == id)
            .Where(crawl => crawl.UserId == userId)
            .Where(crawl => crawl.Url == url)
            .FirstAsync();
    }
}