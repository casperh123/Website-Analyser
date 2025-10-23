using WebsiteAnalyzer.Core.Domain.Uptime;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IDownTimePingRepository : IBaseRepository<DowntimePing>
{
    Task<ICollection<DowntimePing>> GetByWebsiteId(Guid websiteId);
}