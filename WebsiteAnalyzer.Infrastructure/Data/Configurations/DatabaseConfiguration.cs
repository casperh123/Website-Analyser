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

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (environment == "Development")
            {
                string sqliteConnection = connectionString ?? "Data Source=./Data/app.db";
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
                    // Add reliability features
                    npgsqlOptions.EnableRetryOnFailure(3);
                    npgsqlOptions.CommandTimeout(30);
                });

                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        });

        return services;
    }
}