using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Uptime;

namespace WebsiteAnalyser.Api.Endpoints;

public static class UptimeEndpoints {
    public static RouteGroupBuilder MapUptimeEndpoints(this IEndpointRouteBuilder app) {
        RouteGroupBuilder route = app.MapGroup("/uptime")
                                        .RequireAuthorization();

        route.MapGet("/website/{id}", GetByWebsiteId);
    
        return route;
    }

    public static async Task<Ok<ICollection<UptimeStatusDTO>>> GetByWebsiteId(Guid id, IUptimeService uptimeService) {
        DateTime fromDate = DateTime.Now.Subtract(TimeSpan.FromDays(30));
        ICollection<UptimeStatusDTO> uptimeStatuses = await uptimeService.GetByWebsiteAfterDate(id, fromDate);

        return TypedResults.Ok(uptimeStatuses);
    }
}
