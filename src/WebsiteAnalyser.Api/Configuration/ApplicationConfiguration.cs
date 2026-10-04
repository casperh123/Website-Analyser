// Web/Configuration/ApplicationConfiguration.cs

using Microsoft.AspNetCore.Identity;
using WebsiteAnalyzer.Infrastructure;

namespace WebsiteAnalyser.Api.Configuration;

public static class ApplicationConfiguration
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure common middleware
        app.UseHttpsRedirection();
        app.MapStaticAssets();
        app.MapGroup("/account").MapIdentityApi<ApplicationUser>();        
        return app;
    }
}