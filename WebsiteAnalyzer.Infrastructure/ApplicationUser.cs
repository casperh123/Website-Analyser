using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Infrastructure;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public IEnumerable<Website> Websites { get; set; }
}