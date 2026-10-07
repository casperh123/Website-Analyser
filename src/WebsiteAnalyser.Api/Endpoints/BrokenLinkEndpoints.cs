
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyser.Api.User;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyser.Api.Endpoints;

public static class BrokenLinkEndpoints {
    public static RouteGroupBuilder MapBrokenLinkEndpoints(this IEndpointRouteBuilder app) {
        RouteGroupBuilder route = app.MapGroup("/brokenLinks")
                                        .RequireAuthorization();

        route.MapGet("/", GetAll);

        return route;
    }

    public static async Task<Ok<ICollection<BrokenLinkCrawlDTO>>> GetAll(ClaimsPrincipal user, IBrokenLinkService brokenLinkService) {
        Guid userId = user.GetUserId();
        ICollection<BrokenLinkCrawlDTO> brokenLinks = await brokenLinkService.GetCrawlsByUserAsync(userId);

        return TypedResults.Ok(brokenLinks);
    }
}
