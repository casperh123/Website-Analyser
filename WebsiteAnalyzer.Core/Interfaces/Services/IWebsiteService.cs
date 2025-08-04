using WebsiteAnalyzer.Core.Entities.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IWebsiteService
{
    Task<Website> AddWebsite(string url, Guid userId, string? name);
    Task<ICollection<Website>> GetWebsitesByUserId(Guid? userId);
    Task<Website> GetWebsiteByIdAndUserId(Guid id, Guid userId);
    Task DeleteWebsite(string url, Guid userId);
}