using AngleSharp.Dom;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Website> Websites { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Website>().HasKey(w => w.Id);
        builder.Entity<Website>().HasMany<CacheWarmRun>();
    }
}