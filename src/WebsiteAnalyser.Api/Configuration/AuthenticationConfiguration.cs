using Microsoft.AspNetCore.Identity;
using WebsiteAnalyzer.Infrastructure;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyser.Api.Configuration;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddIdentityApiEndpoints<ApplicationUser>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        
        return services;
    }
}