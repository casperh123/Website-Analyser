using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Services.Services;

namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class UptimeMonitorBackgroundService(
    ILogger<UptimeMonitorBackgroundService> logger,
    IServiceProvider serviceProvider)
    : CrawlBackgroundServiceBase(logger, serviceProvider, CrawlAction.Uptime)
{
    protected override async Task ExecuteTaskAsync(
        ScheduledAction scheduledAction, 
        IServiceScope scope, 
        CancellationToken token)
    {
        IUptimeService uptimeService = 
            scope.ServiceProvider.GetRequiredService<IUptimeService>();

        Website website = scheduledAction.Website;

        await uptimeService.Ping(website);
        
        Logger.LogInformation(
            $"Pinged: {website.Url}",
            scheduledAction.Website.Url);
    }
}