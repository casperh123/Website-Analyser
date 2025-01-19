using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Persistence;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Infrastructure.Repositories;

namespace WebsiteAnalyzer.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddScoped<IWebsiteRepository, WebsiteRepository>();
        services.AddScoped<ICacheWarmRepository, CacheWarmRepository>();
        services.AddScoped<ICrawlScheduleRepository, CrawlSheduleRepository>();
        services.AddScoped<IBrokenLinkRepository, BrokenLinkRepository>();
        services.AddScoped<IBrokenLinkCrawlRepository, BrokenLinkCrawlRepository>();
        
        return services;
    }
}