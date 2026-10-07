using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.CacheWarm;

namespace WebsiteAnalyser.Api.Endpoints;

public static class CacheWarmingEndpoints {
    public static RouteGroupBuilder MapCacheWarmingEndpoints(this IEndpointRouteBuilder app) {
        RouteGroupBuilder route = app.MapGroup("/cacheWarms")
                                        .RequireAuthorization();

        route.MapGet("/website/{id}", GetAllByWebsite);

        return route;
    }

    public static async Task<Ok<ICollection<CacheWarmDTO>>> GetAllByWebsite(Guid id, ICacheWarmingService cacheWarmingService) {
        ICollection<CacheWarmDTO> cacheWarms = await cacheWarmingService.GetCacheWarmsByWebsiteId(id);

        return TypedResults.Ok(cacheWarms);
    }
}
