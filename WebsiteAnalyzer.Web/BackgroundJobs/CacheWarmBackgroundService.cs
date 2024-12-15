using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Persistence;


namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class CacheWarmBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5);
    
    public CacheWarmBackgroundService(HttpClient httpClient, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        
        ICrawlScheduleRepository crawlScheduleRepository = scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();
        ICacheWarmRepository cacheWarmRepository = scope.ServiceProvider.GetRequiredService<ICacheWarmRepository>();
        HttpClient httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
        
        ICollection<CrawlSchedule> scheduledItems = await crawlScheduleRepository.GetAllAsync();
        Queue<CrawlSchedule> crawledSchedules = new Queue<CrawlSchedule>(scheduledItems.Where(IsDue));

        List<Task<CacheWarm>> tasks = new List<Task<CacheWarm>>();

        while (crawledSchedules.Any())
        {
            await _semaphore.WaitAsync(stoppingToken);

            CrawlSchedule crawlSchedule = crawledSchedules.Dequeue();
            Task<CacheWarm> task = WarmCacheAsync(httpClient, crawlSchedule);
            
            tasks.Add(task);
            
            if (tasks.Count >= 5)
            {
                Task<CacheWarm> completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
            }
        }
        
        await Task.WhenAll(tasks);

        foreach (Task<CacheWarm> task in tasks)
        {
            CacheWarm cacheWarm = await task;
            CrawlSchedule crawlSchedule = crawledSchedules
                .Where(cs => cs.WebsiteUrl == cacheWarm.WebsiteUrl)
                .First(cs => cs.UserId == cacheWarm.UserId);
            
            crawlSchedule.LastCrawlDate = DateTime.UtcNow;
            
            await cacheWarmRepository.AddAsync(cacheWarm);
            await crawlScheduleRepository.UpdateAsync(crawlSchedule);
        }
    }

    private async Task<CacheWarm> WarmCacheAsync(HttpClient httpClient, CrawlSchedule crawlSchedule)
    {
        ILinkProcessor<Link> linkProcessor = new LinkProcessor(httpClient);
        ModularCrawler<Link> cacheWarmCrawler = new ModularCrawler<Link>(linkProcessor);
        CacheWarm cacheWarm = new CacheWarm()
        {
            Id = Guid.NewGuid(),
            UserId = crawlSchedule.UserId,
            WebsiteUrl = crawlSchedule.WebsiteUrl,
            StartTime = DateTime.UtcNow,
            Schedule = crawlSchedule,
        };

        int linksChecked = await cacheWarmCrawler.CrawlWebsiteAsync(new Link(crawlSchedule.WebsiteUrl)); 
        
        cacheWarm.VisitedPages = linksChecked;
        cacheWarm.EndTime = DateTime.UtcNow;
        
        return cacheWarm;
    }
    
    private bool IsDue(CrawlSchedule crawlSchedule)
    {
        DateTime currentDate = DateTime.UtcNow;
        
        switch (crawlSchedule.Frequency)
        {
            case Frequency.SixHourly:
                return crawlSchedule.LastCrawlDate.AddHours(6) <= currentDate;
            case Frequency.TwelveHourly:
                return crawlSchedule.LastCrawlDate.AddHours(12) <= currentDate;
            case Frequency.Daily:
                return crawlSchedule.LastCrawlDate.AddDays(1) <= currentDate;
            case Frequency.Weekly:
                return crawlSchedule.LastCrawlDate.AddDays(7) <= currentDate;
            default:
                return false;
        }
    }
}