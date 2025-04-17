using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Entities.Website;

namespace WebsiteAnalyzer.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<Website> Websites { get; set; }
    public DbSet<CacheWarm> CacheWarms { get; set; }
    public DbSet<CrawlSchedule> CrawlSchedules { get; set; }
    public DbSet<BrokenLinkCrawl> BrokenLinkCrawls { get; set; }
    public DbSet<BrokenLink> BrokenLinks { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Website>()
            .HasKey(w =>
                new
                {
                    w.Url,
                    w.UserId
                });

        builder.Entity<CacheWarm>()
            .HasKey(cw => cw.Id);

        builder.Entity<CrawlSchedule>()
            .HasKey(cs => new { cs.UserId, cs.Url, cs.Action });

        builder.Entity<BrokenLinkCrawl>()
            .HasKey(blc => new { blc.Id });
    }
}