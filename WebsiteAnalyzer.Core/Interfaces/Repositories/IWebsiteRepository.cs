using WebsiteAnalyzer.Core.Entities.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IWebsiteRepository : IBaseRepository<Website>
{
    Task<Website> GetWebsiteByUrlAndUserAsync(string url, Guid userId);

    Task<bool> ExistsUrlWithUserAsync(string url, Guid userId);
}