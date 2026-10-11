
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyser.Api.User;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyser.Api.Endpoints;

public static class BrokenLinkEndpoints {
    public static RouteGroupBuilder MapBrokenLinkEndpoints(this IEndpointRouteBuilder app) {
        RouteGroupBuilder route = app.MapGroup("/brokenLinks")
                                        .RequireAuthorization();

        route.MapGet("/", Get);

        return route;
    }

    public static async Task<Ok<PagedResponse<BrokenLinkCrawlDTO>>> Get(ClaimsPrincipal user, IBrokenLinkService brokenLinkService, int page = 1, int perPage = 20) {
        Guid userId = user.GetUserId();
        PagedResponse<BrokenLinkCrawlDTO> brokenLinkCrawls = await brokenLinkService.GetCrawlsByUserAsync(userId, page, perPage);

        return TypedResults.Ok(brokenLinkCrawls);
    }
}
