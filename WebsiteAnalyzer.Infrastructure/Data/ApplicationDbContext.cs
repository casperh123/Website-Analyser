using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Website> Websites { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configure Website and CacheWarmRun relationship
        builder.Entity<CacheWarmRun>()
            .HasOne(c => c.Website)
            .WithMany(w => w.CacheWarmRuns)
            .HasForeignKey(c => c.WebsiteId);

        builder.Entity<CacheWarmRun>().HasKey(c => c.Id);
        
        // Configure Identity entities
        builder.Entity<IdentityUserLogin<string>>()
            .HasKey(l => new { l.LoginProvider, l.ProviderKey });

        builder.Entity<IdentityUserRole<string>>()
            .HasKey(r => new { r.UserId, r.RoleId });

        builder.Entity<IdentityUserToken<string>>()
            .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
    }
}