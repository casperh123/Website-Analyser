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

    public WebsiteService(IWebsiteRepository websiteRepository, IScheduleService scheduleService)
    {
        _websiteRepository = websiteRepository;
        _scheduleService = scheduleService;
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
        
        Website website = new Website(url, userId, name);
        
        await _websiteRepository.AddAsync(website);
        await AddScheduledTasks(url, userId);

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

    private async Task AddScheduledTasks(string url, Guid userId)
    {
        await _scheduleService.ScheduleTask(url, userId, CrawlAction.BrokenLink, Frequency.Daily);
        await _scheduleService.ScheduleTask(url, userId, CrawlAction.CacheWarm, Frequency.Daily);
    }
}