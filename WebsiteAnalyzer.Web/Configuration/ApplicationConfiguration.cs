// Web/Configuration/ApplicationConfiguration.cs

using WebsiteAnalyzer.Web.Components;

namespace WebsiteAnalyzer.Web.Configuration;

public static class ApplicationConfiguration
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure environment-specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        // Configure common middleware
        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapStaticAssets();

        // Configure Razor Components
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Configure Identity endpoints
        app.MapAdditionalIdentityEndpoints();

        return app;
    }
}