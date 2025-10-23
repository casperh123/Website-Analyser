using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class DownTimePingRepository : BaseRepository<DowntimePing>, IDownTimePingRepository
{
    public DownTimePingRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ICollection<DowntimePing>> GetByWebsiteId(Guid websiteId)
    {
        return await DbContext.DownTimePings
            .Where(ping => ping.WebsiteId == websiteId)
            .ToListAsync();
    }
}