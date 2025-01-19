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
            }, async (crawlSchedule, token) => { await WarmCacheAsync(crawlSchedule); });
        }
    }


    private async Task WarmCacheAsync(CrawlSchedule crawlSchedule)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        HttpClient httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
        ICrawlScheduleRepository crawlScheduleRepository =
            scope.ServiceProvider.GetRequiredService<ICrawlScheduleRepository>();
        ICacheWarmRepository cacheWarmRepository = scope.ServiceProvider.GetRequiredService<ICacheWarmRepository>();

        crawlSchedule.Status = Status.InProgress;
        crawlSchedule.LastCrawlDate = DateTime.UtcNow;

        await crawlScheduleRepository.UpdateAsync(crawlSchedule);

        try
        {
            ILinkProcessor<Link> linkProcessor = new LinkProcessor(httpClient);
            ModularCrawler<Link> cacheWarmCrawler = new ModularCrawler<Link>(linkProcessor);
            CacheWarm cacheWarm = new CacheWarm()
            {
                Id = Guid.NewGuid(),
                UserId = crawlSchedule.UserId,
                WebsiteUrl = crawlSchedule.Url,
                StartTime = DateTime.UtcNow,
                Schedule = crawlSchedule,
            };

            int linksChecked = 0;

            IAsyncEnumerable<Link> links = cacheWarmCrawler.CrawlWebsiteAsync(new Link(crawlSchedule.Url));

            await foreach (Link link in links)
            {
                linksChecked++;
            }

            cacheWarm.VisitedPages = linksChecked;
            cacheWarm.EndTime = DateTime.UtcNow;

            crawlSchedule.Status = Status.Completed;

            await cacheWarmRepository.AddAsync(cacheWarm);
            await crawlScheduleRepository.UpdateAsync(crawlSchedule);
        }
        catch (Exception ex)
        {
            crawlSchedule.Status = Status.Failed;
            await crawlScheduleRepository.UpdateAsync(crawlSchedule);
        }
    }

    private bool IsDue(CrawlSchedule crawlSchedule)
    {
        if (crawlSchedule.Status is Status.InProgress)
        {
            return false;
        }

        DateTime currentDate = DateTime.UtcNow;

        return crawlSchedule.Frequency switch
        {
            Frequency.SixHourly => crawlSchedule.LastCrawlDate.AddHours(6) <= currentDate,
            Frequency.TwelveHourly => crawlSchedule.LastCrawlDate.AddHours(12) <= currentDate,
            Frequency.Daily => crawlSchedule.LastCrawlDate.AddDays(1) <= currentDate,
            Frequency.Weekly => crawlSchedule.LastCrawlDate.AddDays(7) <= currentDate,
            _ => false
        };
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}