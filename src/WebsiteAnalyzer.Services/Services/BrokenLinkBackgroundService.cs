using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Web.BackgroundJobs;

namespace WebsiteAnalyzer.Services.Services;

public class BrokenLinkBackgroundService(
    ILogger<BrokenLinkBackgroundService> logger,
    IServiceProvider serviceProvider)
    : CrawlBackgroundServiceBase(logger, serviceProvider, CrawlAction.BrokenLink)
{
    protected override async Task ExecuteTaskAsync(
        ScheduledAction scheduledAction, 
        IServiceScope scope, 
        CancellationToken token)
    {
        IBrokenLinkService brokenLinkService = 
            scope.ServiceProvider.GetRequiredService<IBrokenLinkService>();

        await brokenLinkService.FindBrokenLinks(scheduledAction.Website, null, token);
        
        Logger.LogInformation(
            "Completed crawl of {Url}.",
            scheduledAction.Website.Url);
    }
}