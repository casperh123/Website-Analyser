using WebsiteAnalyzer.Core.Contracts;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class BrokenLinkBackgroundService(
    ILogger<BrokenLinkBackgroundService> logger,
    IServiceProvider serviceProvider)
    : CrawlBackgroundServiceBase(logger, serviceProvider, CrawlAction.BrokenLink)
{
    protected override async Task ExecuteCrawlTaskAsync(
        ScheduledAction scheduledAction, 
        IServiceScope scope, 
        CancellationToken token)
    {
        IBrokenLinkService brokenLinkService = 
            scope.ServiceProvider.GetRequiredService<IBrokenLinkService>();

        int brokenLinks = 0;
        int totalLinks = 0;

        IProgress<Progress> progress = new Progress<Progress>(p =>
        {
            totalLinks = p.LinksChecked;
        });

        try
        {
            IAsyncEnumerable<BrokenLinkDTO> linksFound =
                brokenLinkService.FindBrokenLinks(scheduledAction.Website, progress, token);

            await foreach (BrokenLinkDTO unused in linksFound)
            {
                brokenLinks++;
            }
            
            Logger.LogInformation(
                "Completed crawl of {Url}. Found {BrokenCount} broken links out of {TotalCount} total links",
                scheduledAction.Website.Url,
                brokenLinks,
                totalLinks);
        }
        catch (Exception e)
        {
        }
    }
}