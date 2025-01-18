using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Entities.Website;

namespace WebsiteAnalyzer.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private readonly ILogger<ApplicationDbContext> _logger;
    public static int _connectionCount = 0;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    public DbSet<Website> Websites { get; set; }
    public DbSet<CacheWarm> CacheWarms { get; set; }
    public DbSet<CrawlSchedule> CrawlSchedules { get; set; }

    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Log connection count for debugging
        _logger.LogDebug("Database connection #{Count} configured", 
            Interlocked.Increment(ref _connectionCount));
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Website entity
        builder.Entity<Website>()
            .HasKey(w =>
                new
                {
                    w.Url,
                    w.UserId
                });

        // Configure CacheWarm and Website relationship
        builder.Entity<CacheWarm>()
            .HasKey(c => c.Id); // Use GUID for CacheWarm primary key

        builder.Entity<CrawlSchedule>()
            .HasKey(cs => new { cs.UserId, WebsiteUrl = cs.Url });

        builder.Entity<BrokenLinkCrawl>()
            .HasKey(blc => new { blc.Id, blc.Url, blc.UserId });

        // Configure Identity entities to use GUID
        builder.Entity<IdentityUserLogin<Guid>>()
            .HasKey(l => new { l.LoginProvider, l.ProviderKey });

        builder.Entity<IdentityUserRole<Guid>>()
            .HasKey(r => new { r.UserId, r.RoleId });

        builder.Entity<IdentityUserToken<Guid>>()
            .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
    }
}