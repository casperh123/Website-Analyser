using WebsiteAnalyzer.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace WebsiteAnalyzer.Infrastructure;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public IEnumerable<Website> Websites { get; set; }
}