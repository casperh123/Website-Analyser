using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IWebsiteService
{
    Task<Website> GetOrAddWebsite(string url, Guid userId);
}