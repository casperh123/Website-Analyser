using WebsiteAnalyzer.Core.DataTransferObject;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class BrokenLinkBackgroundService : CrawlBackgroundServiceBase
{
    public BrokenLinkBackgroundService(
        ILogger<BrokenLinkBackgroundService> logger, 
        IServiceProvider serviceprovider) : base(logger, serviceprovider, CrawlAction.BrokenLink)
    {
    }

    protected override async Task ExecuteCrawlTaskAsync(CrawlSchedule schedule, IServiceScope scope, CancellationToken token)
    {
        IBrokenLinkService brokenLinkService = scope.ServiceProvider.GetService<IBrokenLinkService>();
        BrokenLinkCrawlDTO brokenLinkCrawl= await brokenLinkService.StartCrawl(schedule.Url, schedule.UserId);
        IAsyncEnumerable<BrokenLinkDTO> linksFound = brokenLinkService.FindBrokenLinks(brokenLinkCrawl.Url, brokenLinkCrawl.Id, token);
        int brokenLinks = 0;

        await foreach (BrokenLinkDTO brokenLink in linksFound)
        {
            brokenLinks++;
        };
        
        await brokenLinkService.EndCrawl(brokenLinkCrawl, brokenLinks, schedule.UserId);
    }
}