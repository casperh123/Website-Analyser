using WebsiteAnalyzer.Web.BackgroundJobs;

namespace WebsiteAnalyzer.Web.Configuration;

public static class BackgroundServiceConfiguration
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

        return services;
    }
}