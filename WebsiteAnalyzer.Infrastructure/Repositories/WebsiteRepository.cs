using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Persistence;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class WebsiteRepository : BaseRepository<Website>, IWebsiteRepository
{
    public WebsiteRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Website> GetWebsiteByUrlAsync(string url)
    {
        return await _dbContext.Websites
            .Where(w => w.Url == url)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsUrlAsync(string url)
    {
        return await _dbContext.Websites
            .AnyAsync(w => w.Url == url)
            .ConfigureAwait(false);
    }
}