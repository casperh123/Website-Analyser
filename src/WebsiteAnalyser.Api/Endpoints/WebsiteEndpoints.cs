using System.Runtime.CompilerServices;
using WebsiteAnalyser.Api.ClaimsPrincipal;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyser.Api.Endpoints;

public static class WebsiteEndpoints
{
    public static RouteGroupBuilder MapWebsiteEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/websites");

        group.MapGet("/", async (System.Security.Claims.ClaimsPrincipal user, IWebsiteService sites) =>
                TypedResults.Ok(await sites.GetWebsitesByUserId(user.GetUserId())))
            .RequireAuthorization();
        
        return group;
    }
}