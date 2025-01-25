using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.Core.Persistence;

public interface IWebsiteRepository : IBaseRepository<Website>
{
    Task<Website> GetWebsiteByUrlAndUserAsync(string url, Guid userId);

    Task<bool> ExistsUrlWithUserAsync(string url, Guid userId);
}