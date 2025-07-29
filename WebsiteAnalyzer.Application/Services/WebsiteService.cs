using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Exceptions;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class WebsiteService : IWebsiteService
{
    private readonly IWebsiteRepository _websiteRepository;
    private readonly IScheduleService _scheduleService;
    private readonly HttpClient _httpClient;

    public WebsiteService(IWebsiteRepository websiteRepository, IScheduleService scheduleService, HttpClient httpClient)
    {
        _websiteRepository = websiteRepository;
        _scheduleService = scheduleService;
        _httpClient = httpClient;
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

    public async Task<Website> AddWebsite(string url, Guid userId, string? name)
    {
        if (await _websiteRepository.ExistsUrlWithUserAsync(url, userId))
        {
            throw new AlreadyExistsException($"{url} is already added");
        }

        await VerifyWebsite(url);
        
        Website website = new Website(url, userId, name);
        
        await _websiteRepository.AddAsync(website);
        await AddScheduledTasks(website);

        return website;
    }

    public async Task<ICollection<Website>> GetWebsitesByUserId(Guid? userId)
    {
        if (userId is null)
        {
            return [];
        }
            
        ICollection<Website> websites = await _websiteRepository.GetAllByUserId(userId.Value);

        return websites;
    }

    public async Task<Website> GetWebsiteByIdAndUserId(Guid id, Guid userId)
    {
        Website website = await _websiteRepository.GetByIdAndUserId(id, userId) 
                ?? throw new NotFoundException($"Website with ID: {id} not found.");
        
        return website;
    }

    public async Task DeleteWebsite(string url, Guid userId)
    {
        await _scheduleService.DeleteTasksByUrlAndUserId(url, userId);
        await _websiteRepository.DeleteByUrlAndUserId(url, userId);
    }

    private async Task VerifyWebsite(string url)
    {
        try
        {
            await _httpClient.GetAsync(url);
        }
        catch (Exception e)
        {
            throw new UrlException($"Could not verify URL: {url}");
        }
    }

    private async Task AddScheduledTasks(Website website)
    {
        const int cacheWarmDelayMinutes = 10;
    
        DateTime cacheWarmTime = DateTime.UtcNow;
        DateTime brokenLinkTime = cacheWarmTime.AddMinutes(cacheWarmDelayMinutes);
    
        await _scheduleService.ScheduleTask(website, CrawlAction.CacheWarm, Frequency.Daily, cacheWarmTime);
        await _scheduleService.ScheduleTask(website, CrawlAction.BrokenLink, Frequency.Daily, brokenLinkTime);
    }
}