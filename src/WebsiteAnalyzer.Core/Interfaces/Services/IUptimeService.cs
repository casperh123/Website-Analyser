using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Uptime;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IUptimeService
{
    Task<DowntimePing> Ping(Guid websiteId);
    Task<ICollection<UptimeStatusDTO>> GetByWebsiteAfterDate(Guid websiteId, DateTime afterDate);

}