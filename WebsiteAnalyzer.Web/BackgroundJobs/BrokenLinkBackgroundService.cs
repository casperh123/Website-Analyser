using WebsiteAnalyzer.Core.DataTransferObject;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Events;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class BrokenLinkBackgroundService : CrawlBackgroundServiceBase
{
    public BrokenLinkBackgroundService(
        ILogger<BrokenLinkBackgroundService> logger, 
        IServiceProvider serviceProvider) 
        : base(logger, serviceProvider, CrawlAction.BrokenLink)
    {
    }

    protected override async Task ExecuteCrawlTaskAsync(
        CrawlSchedule schedule, 
        IServiceScope scope, 
        CancellationToken token)
    {
        IBrokenLinkService brokenLinkService = 
            scope.ServiceProvider.GetRequiredService<IBrokenLinkService>();

        int brokenLinks = 0;
        int totalLinks = 0;

        void OnProgressUpdated(object? sender, CrawlProgressEventArgs e)
        {
            totalLinks = e.LinksChecked;
            Logger.LogInformation(
                "Crawl progress for {Url}: {Checked} links checked", 
                schedule.Url, 
                e.LinksChecked);
        }

        try
        {
            BrokenLinkCrawlDTO brokenLinkCrawl = 
                await brokenLinkService.StartCrawl(schedule.Url, schedule.UserId);

            brokenLinkService.ProgressUpdated += OnProgressUpdated;

            IAsyncEnumerable<BrokenLinkDTO> linksFound = 
                brokenLinkService.FindBrokenLinks(brokenLinkCrawl.Url, brokenLinkCrawl.Id, token);

            await foreach (BrokenLinkDTO brokenLink in linksFound.WithCancellation(token))
            {
                brokenLinks++;
                Logger.LogInformation(
                    "Found broken link #{Count} at {BrokenUrl}", 
                    brokenLinks, 
                    brokenLink.ReferringPage);
            }

            await brokenLinkService.EndCrawl(brokenLinkCrawl, totalLinks, schedule.UserId);
            
            Logger.LogInformation(
                "Completed crawl of {Url}. Found {BrokenCount} broken links out of {TotalCount} total links",
                schedule.Url,
                brokenLinks,
                totalLinks);
        }
        finally
        {
            brokenLinkService.ProgressUpdated -= OnProgressUpdated;
        }
    }
}