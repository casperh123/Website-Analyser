using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Persistence;


namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class CacheWarmBackgroundService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPeriodicTimer _timer;
    
    public CacheWarmBackgroundService(IServiceProvider serviceProvider, IPeriodicTimer timer)
    {
        _serviceProvider = serviceProvider;
        _timer = timer;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        ICrawlScheduleRepository crawlScheduleRepository =
            scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();

        while (!cancellationToken.IsCancellationRequested
               && await _timer.WaitForNextTickAsync(cancellationToken))
        {
            ICollection<CrawlSchedule> scheduledItems = await crawlScheduleRepository.GetAllAsync();
            IEnumerable<CrawlSchedule> dueSchedules = scheduledItems.Where(IsDue);

            int maxDegreeOfParallelism = 5;

            await Parallel.ForEachAsync(dueSchedules, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = cancellationToken
            }, async (crawlSchedule, token) =>
            {
                await WarmCacheAsync(crawlSchedule);
            });
        }
    }


    private async Task WarmCacheAsync(CrawlSchedule crawlSchedule)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        HttpClient httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
        ICrawlScheduleRepository crawlScheduleRepository = scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();
        ICacheWarmRepository cacheWarmRepository = scope.ServiceProvider.GetRequiredService<ICacheWarmRepository>();

        crawlSchedule.Status = Status.InProgress;
        crawlSchedule.LastCrawlDate = DateTime.UtcNow;

        await crawlScheduleRepository.UpdateAsync(crawlSchedule);
        
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
        
        crawlSchedule.Status = Status.Completed;
        
        await cacheWarmRepository.AddAsync(cacheWarm);
        await crawlScheduleRepository.UpdateAsync(crawlSchedule);
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
    
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
} 