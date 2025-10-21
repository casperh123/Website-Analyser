using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Domain.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IUptimeService
{
    Task<DownTimePing> Ping(Website website);
}