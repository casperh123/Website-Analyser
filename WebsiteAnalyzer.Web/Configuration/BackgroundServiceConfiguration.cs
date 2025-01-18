using WebsiteAnalyzer.Web.BackgroundJobs;

namespace WebsiteAnalyzer.Web.Configuration;

public static class BackgroundServiceConfiguration
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        // Register the periodic timer as a singleton
        services.AddSingleton<IPeriodicTimer, HourlyTimer>();

        // Configure background service options
        services.Configure<HostOptions>(options => 
        { 
            options.ServicesStartConcurrently = true;
        });

        // Register the background service
        services.AddHostedService<CacheWarmBackgroundService>();

        return services;
    }
}