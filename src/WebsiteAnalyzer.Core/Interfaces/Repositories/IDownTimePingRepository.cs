using WebsiteAnalyzer.Core.Domain.Uptime;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IDownTimePingRepository : IBaseRepository<DownTimePing>
{
    Task<ICollection<DownTimePing>> GetByWebsiteId(Guid websiteId);
}