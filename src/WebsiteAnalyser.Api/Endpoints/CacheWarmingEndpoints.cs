using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.CacheWarm;

namespace WebsiteAnalyser.Api.Endpoints;

public static class CacheWarmingEndpoints {
    public static RouteGroupBuilder MapCacheWarmingEndpoints(this IEndpointRouteBuilder app) {
        RouteGroupBuilder route = app.MapGroup("/")
                                        .RequireAuthorization();

        route.MapGet("/websites/{id}/cachewarms", GetByWebsiteId);

        return route;
    }

    public static async Task<Ok<PagedResponse<CacheWarmDTO>>> GetByWebsiteId(Guid id, ICacheWarmingService cacheWarmingService, int page = 1, int perPage = 20) {
        PagedResponse<CacheWarmDTO> cacheWarms = await cacheWarmingService.GetCacheWarmsByWebsiteId(id, page, perPage);

        return TypedResults.Ok(cacheWarms);
    }
}
