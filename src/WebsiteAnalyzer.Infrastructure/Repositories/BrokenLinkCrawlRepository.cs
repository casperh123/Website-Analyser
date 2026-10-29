using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class BrokenLinkCrawlRepository : BaseRepository<BrokenLinkCrawl>, IBrokenLinkCrawlRepository
{
    public BrokenLinkCrawlRepository(ApplicationDbContext dbContext) : base(dbContext)
    {

    }

    public async Task<ICollection<BrokenLinkCrawl>> GetByWebsiteId(Guid websiteId)
    {
        return await DbContext.BrokenLinkCrawls
            .Where(crawl => crawl.WebsiteId == websiteId)
            .Include(crawl => crawl.BrokenLinks)
            .OrderByDescending(crawl => crawl.Date)
            .ToListAsync();
    }

    public async Task<PagedResponse<BrokenLinkCrawl>> GetByWebsiteId(Guid websiteId, int page = 1, int perPage = 20)
    {
        IEnumerable<BrokenLinkCrawl> crawls = await DbContext.BrokenLinkCrawls
            .Where(crawl => crawl.WebsiteId == websiteId)
            .OrderByDescending(crawl => crawl.Date)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .Include(crawl => crawl.BrokenLinks)
            .ToListAsync();

        int count = await DbContext.BrokenLinkCrawls
            .Where(crawl => crawl.WebsiteId == websiteId)
            .CountAsync();

        return new PagedResponse<BrokenLinkCrawl>(crawls, page, perPage, count);
    }
}
