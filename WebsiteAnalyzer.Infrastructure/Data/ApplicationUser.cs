using Microsoft.AspNetCore.Identity;
using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Web.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public IEnumerable<Website> Websites { get; set; }
}