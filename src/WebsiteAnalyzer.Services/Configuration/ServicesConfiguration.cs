using WebsiteAnalyzer.Services.Services;

namespace WebsiteAnalyzer.Services.Configuration;

public static class ServiceConfiguration
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {

        // Configure background service options
        services.Configure<HostOptions>(options => 
        { 
            options.ServicesStartConcurrently = true;
        });
        
        // Register the background service
        services.AddHostedService<CacheWarmBackgroundService>();
        services.AddHostedService<BrokenLinkBackgroundService>();
        services.AddHostedService<UptimeMonitorBackgroundService>();
        services.AddHostedService<OrderCheckBackgroundService>();

        return services;
    }
}