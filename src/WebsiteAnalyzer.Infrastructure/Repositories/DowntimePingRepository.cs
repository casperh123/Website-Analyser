using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class DowntimePingRepository : BaseRepository<DowntimePing>, IDowntimePingRepository
{
    public DowntimePingRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ICollection<DowntimePing>> GetByWebsiteId(Guid websiteId)
    {
        return await DbContext.DownTimePings
            .Where(ping => ping.WebsiteId == websiteId)
            .ToListAsync();
    }

    public async Task<ICollection<DowntimePing>> GetByWebsiteIdAfterDate(Guid websiteId, DateTime afterDate)
    {
        return await DbContext.DownTimePings
            .Where(ping => ping.WebsiteId == websiteId)
            .Where(ping => ping.TimeRecorded > afterDate)
            .ToListAsync();
    }
}