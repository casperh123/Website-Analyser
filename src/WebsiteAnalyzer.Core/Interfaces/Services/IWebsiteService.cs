using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IWebsiteService
{
    Task<WebsiteDTO> AddWebsite(string url, Guid userId, string? name);
    Task<ICollection<WebsiteDTO>> GetWebsitesByUserId(Guid? userId);
    Task<WebsiteDTO?> GetById(Guid id);
    Task DeleteWebsite(string url, Guid userId);
}