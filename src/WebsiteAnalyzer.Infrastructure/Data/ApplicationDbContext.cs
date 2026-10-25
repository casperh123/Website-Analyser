using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Domain.OrderChecks;
using WebsiteAnalyzer.Core.Domain.Uptime;

namespace WebsiteAnalyzer.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<Website> Websites { get; set; }
    public DbSet<CacheWarm> CacheWarms { get; set; }
    public DbSet<ScheduledAction> ScheduledActions { get; set; }
    public DbSet<BrokenLinkCrawl> BrokenLinkCrawls { get; set; }
    public DbSet<BrokenLink> BrokenLinks { get; set; }
    public DbSet<DowntimePing> DownTimePings { get; set; }
    public DbSet<EmailSubscription> EmailSubcriptions { get; set; }
    public DbSet<OrderCheck> OrderChecks { get; set; }
    public DbSet<OrderCheckKeys> OrderCheckKeys { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<OrderCheckKeys>()
            .HasKey(e => new { e.WebsiteId });
        
        // Configure Website entity
        builder.Entity<Website>()
            .HasIndex(w => new { w.Url, w.UserId })
            .IsUnique();

        builder.Entity<Website>()
            .HasOne(w => w.OrderCheckKeys)
            .WithOne(k => k.Website)
            .HasForeignKey<OrderCheckKeys>(k => k.WebsiteId);

        // Configure CacheWarm and Website relationship
        builder.Entity<CacheWarm>()
            .HasKey(cw => cw.Id);

        builder.Entity<BrokenLinkCrawl>()
            .HasKey(blc => new { blc.Id });

        builder.Entity<DowntimePing>()
            .HasKey(ping => ping.Id);

        builder.Entity<EmailSubscription>()
            .HasKey(s =>
            new {
                s.WebsiteId, s.SubscripedAction, s.Email
            });

        // Configure Identity entities to use GUID
        builder.Entity<IdentityUserLogin<Guid>>()
            .HasKey(l => new { l.LoginProvider, l.ProviderKey });

        builder.Entity<IdentityUserRole<Guid>>()
            .HasKey(r => new { r.UserId, r.RoleId });

        builder.Entity<IdentityUserToken<Guid>>()
            .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
    }
}
