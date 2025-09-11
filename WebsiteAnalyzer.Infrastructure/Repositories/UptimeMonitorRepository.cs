using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class UptimeMonitorRepository : BaseRepository<UptimeMonitor>, IUptimeRepository
{
    public UptimeMonitorRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}