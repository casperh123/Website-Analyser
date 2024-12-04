using AngleSharp.Dom;
using BrokenLinkChecker.crawler;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface IWebsiteService
{
    Task<Website> GetOrAddWebsite(string url);
}

public class WebsiteService : IWebsiteService
{
    private readonly IWebsiteRepository _websiteRepository;

    public WebsiteService(IWebsiteRepository websiteRepository)
    {
        _websiteRepository = websiteRepository;
    }

    public async Task<Website> GetOrAddWebsite(string url)
    {
        if (await _websiteRepository.ExistsUrlAsync(url))
        {
            return await _websiteRepository.GetWebsiteByUrlAsync(url);
        }

        Website website = new Website { Url = url };
        await _websiteRepository.AddAsync(website);
        return website;
    }
}