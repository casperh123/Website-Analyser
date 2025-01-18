using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Application.Services;

public class WebsiteService : IWebsiteService
{
    private readonly IWebsiteRepository _websiteRepository;

    public WebsiteService(IWebsiteRepository websiteRepository)
    {
        _websiteRepository = websiteRepository;
    }

    public async Task<Website> GetOrAddWebsite(string url, Guid userId)
    {
        if (await _websiteRepository.ExistsUrlWithUserAsync(url, userId).ConfigureAwait(false))
        {
            return await _websiteRepository.GetWebsiteByUrlAndUserAsync(url, userId).ConfigureAwait(false);
        }

        Website website = new Website(url, userId);

        await _websiteRepository.AddAsync(website).ConfigureAwait(false);
        return website;
    }
}