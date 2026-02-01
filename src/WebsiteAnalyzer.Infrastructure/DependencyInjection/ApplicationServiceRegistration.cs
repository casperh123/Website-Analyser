using Microsoft.Extensions.DependencyInjection;
using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Infrastructure.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IWebsiteService, WebsiteService>();
        services.AddScoped<ICacheWarmingService, CacheWarmingService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IBrokenLinkService, BrokenLinkService>();
        services.AddScoped<IOrderCheckService, OrderCheckService>();
        services.AddScoped<IUptimeService, UptimeService>();
        services.AddSingleton<MailService>();

        return services;
    }
}