using WebsiteAnalyzer.Core.Domain.Uptime;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IDowntimePingRepository : IBaseRepository<DowntimePing>
{
    Task<ICollection<DowntimePing>> GetByWebsiteId(Guid websiteId);
    Task<ICollection<DowntimePing>> GetByWebsiteIdAfterDate(Guid websiteId, DateTime afterDate);
}