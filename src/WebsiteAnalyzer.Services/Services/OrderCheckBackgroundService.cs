using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Web.BackgroundJobs;

namespace WebsiteAnalyzer.Services.Services;

public class OrderCheckBackgroundService : CrawlBackgroundServiceBase
{
    public OrderCheckBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OrderCheckBackgroundService> logger) : base(logger, serviceProvider, CrawlAction.OrderCheck)
    {
    }

    protected override async Task ExecuteTaskAsync(
        ScheduledAction action,
        IServiceScope scope,
        CancellationToken token)
    {
        IOrderCheckService checkService = scope.ServiceProvider.GetService<IOrderCheckService>()!;
        
        await checkService.CheckOrder(action.WebsiteId);
    }
}