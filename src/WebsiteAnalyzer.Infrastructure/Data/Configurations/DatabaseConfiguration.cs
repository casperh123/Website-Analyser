// Infrastructure/Data/DatabaseConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebsiteAnalyzer.Infrastructure.Data.Configurations;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase) || 
                string.IsNullOrEmpty(environment))
            {
                string sqliteConnection = connectionString ?? "Data Source=../WebsiteAnalyzer.Web/Data/app.db";
                options.UseSqlite(sqliteConnection);
            }
            else
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Production database connection string not found");
                }
                
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(30);
                    npgsqlOptions.MaxBatchSize(10);
                    npgsqlOptions.EnableRetryOnFailure(3);
                });
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        });

        return services;
    }
}