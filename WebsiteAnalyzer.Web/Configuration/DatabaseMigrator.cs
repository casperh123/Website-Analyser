using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using WebsiteAnalyzer.Infrastructure;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Web.Configuration;

public static class DatabaseMigrator
{
    public static async Task MigrateDatabase(WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            // Migrate application database
            logger.LogInformation("Starting application database migration");

            IDbContextFactory<ApplicationDbContext> dbContextFactory = 
                services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

            await using ApplicationDbContext appContext = await dbContextFactory.CreateDbContextAsync();
            
            await appContext.Database.EnsureCreatedAsync();
            
            try
            {
                await appContext.Database.MigrateAsync();
                logger.LogInformation("Application database migration completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "An issue occurred during application database migration");
                // We continue execution to try the identity migration
            }

            // Migrate identity database
            logger.LogInformation("Starting identity database migration");
            
            // Get the identity context type that you're using
            ApplicationIdentityDbContext identityContext = services.GetRequiredService<ApplicationIdentityDbContext>();
            await identityContext.Database.EnsureCreatedAsync();
            
            try
            {
                await identityContext.Database.MigrateAsync();
                logger.LogInformation("Identity database migration completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "An issue occurred during identity database migration");
                // We continue execution
            }
            
            logger.LogInformation("All database migrations have been processed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A critical error occurred while migrating the databases");
            throw;
        }
    }
}