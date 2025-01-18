using WebsiteAnalyzer.Core.Entities;
using Microsoft.AspNetCore.Identity;
using WebsiteAnalyzer.Core.Entities.Website;

namespace WebsiteAnalyzer.Infrastructure;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<Guid>
{
    public IEnumerable<Website> Websites { get; set; }
}