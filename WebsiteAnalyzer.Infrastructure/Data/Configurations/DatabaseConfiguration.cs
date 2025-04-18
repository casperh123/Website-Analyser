// Infrastructure/Data/DatabaseConfiguration.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        // Register Identity context as scoped (for Identity to use)
        services.AddDbContext<ApplicationIdentityDbContext>(options => 
            ConfigureDatabase(options, configuration));
        
        // Register app context as factory (for repositories to use)
        services.AddPooledDbContextFactory<ApplicationDbContext>(options => 
            ConfigureDatabase(options, configuration));
    
        return services;
    
        static void ConfigureDatabase(DbContextOptionsBuilder options, IConfiguration configuration)
        {
            string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
        
            if (environment == "Development")
            {
                options.UseSqlite(connectionString ?? "Data Source=./Data/app.db");
            }
            else
            {
                options.UseNpgsql(connectionString, o => {
                    o.CommandTimeout(30);
                    o.MaxBatchSize(10);
                    o.EnableRetryOnFailure(3);
                });
            }
        }
    }
}