using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Domain.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IUptimeService
{
    Task<ICollection<DowntimePing>> GetDowntimePingsByWebsiteId(Guid websiteId);
    Task<DowntimePing> Ping(Website website);
}