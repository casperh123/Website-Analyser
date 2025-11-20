using WebsiteAnalyzer.Services.Services;
using WebsiteAnalyzer.Web.BackgroundJobs;

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

        return services;
    }
}