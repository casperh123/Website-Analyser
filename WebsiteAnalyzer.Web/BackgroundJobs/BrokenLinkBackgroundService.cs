using WebsiteAnalyzer.Core.Contracts;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain;
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
        ScheduledAction scheduledAction, 
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
        }

        try
        {
            BrokenLinkCrawlDTO brokenLinkCrawl = 
                await brokenLinkService.StartCrawl(scheduledAction.Website.Url, scheduledAction.Website.UserId);

            brokenLinkService.ProgressUpdated += OnProgressUpdated;

            IAsyncEnumerable<BrokenLinkDTO> linksFound = 
                brokenLinkService.FindBrokenLinks(brokenLinkCrawl.Url, brokenLinkCrawl.Id, token);

            await foreach (BrokenLinkDTO unused in linksFound)
            {
                brokenLinks++;
            }

            await brokenLinkService.EndCrawl(brokenLinkCrawl, totalLinks, scheduledAction.Website.UserId);
            
            Logger.LogInformation(
                "Completed crawl of {Url}. Found {BrokenCount} broken links out of {TotalCount} total links",
                scheduledAction.Website.Url,
                brokenLinks,
                totalLinks);
        }
        finally
        {
            brokenLinkService.ProgressUpdated -= OnProgressUpdated;
        }
    }
}