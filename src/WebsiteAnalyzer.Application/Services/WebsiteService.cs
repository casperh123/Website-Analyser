using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Exceptions;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Website;

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

    public async Task<WebsiteDTO> AddWebsite(string url, Guid userId, string? name)
    {
        await VerifyWebsite(url);
        
        Website website = new Website(url, userId, name);
        
        await _websiteRepository.AddAsync(website);
        await AddScheduledTasks(website);

        return WebsiteDTO.From(website);
    }

    public async Task<ICollection<WebsiteDTO>> GetWebsitesByUserId(Guid? userId)
    {
        if (userId is null) return [];

        ICollection<Website> websites = await _websiteRepository.GetAllByUserId(userId.Value);
        
        return [.. websites.Select(WebsiteDTO.From)];
    }

    public async Task<WebsiteDTO> GetWebsiteByIdAndUserId(Guid id, Guid userId)
    {
        Website website = await _websiteRepository.GetByIdAndUserId(id, userId)
                          ?? throw new NotFoundException($"Website with ID: {id} not found.");
        
        return WebsiteDTO.From(website);
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
        TimeSpan brokenLinkOffset = TimeSpan.FromMinutes(15);
    
        await _scheduleService.ScheduleAction(website.Id, CrawlAction.CacheWarm, Frequency.SixHourly);
        await _scheduleService.ScheduleAction(website.Id, CrawlAction.BrokenLink, Frequency.Daily, brokenLinkOffset);
        await _scheduleService.ScheduleAction(website.Id, CrawlAction.Uptime, Frequency.Minutely);
    }
}