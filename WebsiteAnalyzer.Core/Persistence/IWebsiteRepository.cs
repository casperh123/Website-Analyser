using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Core.Persistence;

public interface IWebsiteRepository : IBaseRepository<Website>
{
    Task<Website> GetWebsiteByUrlAsync(string url);

    Task<bool> ExistsUrlAsync(string url);
}