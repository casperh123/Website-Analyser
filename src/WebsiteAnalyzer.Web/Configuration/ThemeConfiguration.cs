using Radzen;

namespace WebsiteAnalyzer.Web.Configuration;

public static class ThemeConfiguration
{
    public static IServiceCollection AddThemeServices(this IServiceCollection services)
    {
        // Register the theme service for dependency injection
        services.AddScoped<ThemeService>();

        // Configure Radzen's cookie-based theme service
        services.AddRadzenCookieThemeService(options =>
        {
            options.Name = "WebsiteAnalyzerTheme";
            options.Duration = TimeSpan.FromDays(365);
        });

        return services;
    }
}