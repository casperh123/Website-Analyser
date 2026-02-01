using Microsoft.AspNetCore.Identity.UI.Services;
using WebsiteAnalyzer.Services.Services;
using WebsiteAnalyzer.Web.BackgroundJobs;
using WebsiteAnalyzer.Web.Services;

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

        
        services.AddTransient<IEmailSender, MailSenderProvider>();

        // Register the background service
        services.AddHostedService<CacheWarmBackgroundService>();
        services.AddHostedService<BrokenLinkBackgroundService>();
        services.AddHostedService<UptimeMonitorBackgroundService>();
        services.AddHostedService<OrderCheckBackgroundService>();
        services.AddHostedService<AppartmentBackgroundService>();

        return services;
    }
}