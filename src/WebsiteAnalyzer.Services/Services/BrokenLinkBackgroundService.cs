using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.ScheduledAction;

namespace WebsiteAnalyzer.Services.Services;

public class BrokenLinkBackgroundService(
    ILogger<BrokenLinkBackgroundService> logger,
    IServiceProvider serviceProvider)
    : CrawlBackgroundServiceBase(logger, serviceProvider, CrawlAction.BrokenLink)
{
    protected override async Task ExecuteTaskAsync(
        ScheduledActionDTO scheduledAction, 
        IServiceScope scope, 
        CancellationToken token)
    {
        IBrokenLinkService brokenLinkService = 
            scope.ServiceProvider.GetRequiredService<IBrokenLinkService>();

        await brokenLinkService.FindBrokenLinks(scheduledAction.Website.Id, null, token);
        
        Logger.LogInformation(
            "Completed crawl of {Url}.",
            scheduledAction.Website.Url);
    }
}
