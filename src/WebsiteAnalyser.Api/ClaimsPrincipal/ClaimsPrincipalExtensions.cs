using System.Security.Claims;

namespace WebsiteAnalyser.Api.ClaimsPrincipal;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this System.Security.Claims.ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("No user id in token"));
}