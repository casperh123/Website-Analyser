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

        IAsyncEnumerable<BrokenLinkDTO> linksFound =
            brokenLinkService.FindBrokenLinks(scheduledAction.Website, null, token);

        await foreach (BrokenLinkDTO unused in linksFound)
        {
        }
        
        Logger.LogInformation(
            "Completed crawl of {Url}.",
            scheduledAction.Website.Url);
    }
}