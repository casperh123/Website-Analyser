using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class WebsiteRepository : BaseRepository<Website>, IWebsiteRepository
{
    public WebsiteRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(dbContextFactory)
    {
    }

    public async Task<Website> GetWebsiteByUrlAndUserAsync(string url, Guid userId)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();
        
        return await dbContext.Websites
            .Where(w => w.UserId == userId)
            .Where(w => w.Url == url)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsUrlWithUserAsync(string url, Guid userId)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.Websites
            .Where(w => w.UserId == userId)
            .AnyAsync(w => w.Url == url)
            .ConfigureAwait(false);
    }
}