using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Website> Websites { get; set; }
    public DbSet<CacheWarm> CacheWarms { get; set; }
    public DbSet<CrawlSchedule> CrawlSchedules { get; set; }

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
            .HasKey(cs => new { cs.UserId, cs.WebsiteUrl });

        // Configure Identity entities to use GUID
        builder.Entity<IdentityUserLogin<Guid>>()
            .HasKey(l => new { l.LoginProvider, l.ProviderKey });

        builder.Entity<IdentityUserRole<Guid>>()
            .HasKey(r => new { r.UserId, r.RoleId });

        builder.Entity<IdentityUserToken<Guid>>()
            .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
    }
}