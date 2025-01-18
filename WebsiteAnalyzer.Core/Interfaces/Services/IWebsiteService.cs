using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IWebsiteService
{
    Task<Website> GetOrAddWebsite(string url, Guid userId);
}