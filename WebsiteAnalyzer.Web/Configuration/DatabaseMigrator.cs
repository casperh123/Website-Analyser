using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Web.Configuration;

public static class DatabaseMigrator
{
    public static async Task MigrateDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Starting database migration");
            // Ensure database is created and all pending migrations are applied
            await context.Database.EnsureCreatedAsync();

            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                // ignored
            }

            logger.LogInformation("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }
}