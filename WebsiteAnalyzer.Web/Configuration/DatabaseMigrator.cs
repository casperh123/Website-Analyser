using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Web.Configuration;

public static class DatabaseMigrator
{
    public static async Task MigrateDatabase(WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        try
        {
            ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
            ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Starting database migration");
            // Ensure database is created and all pending migrations are applied
            await context.Database.EnsureCreatedAsync();

            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception)
            {
                // ignored
            }

            logger.LogInformation("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }
}