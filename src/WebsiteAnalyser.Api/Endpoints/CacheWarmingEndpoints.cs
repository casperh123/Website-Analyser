using Crawl.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyser.Api.DTOs;
using WebsiteAnalyser.Api.Streaming;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.CacheWarm;

namespace WebsiteAnalyser.Api.Endpoints;

public static class CacheWarmingEndpoints {
    public static RouteGroupBuilder MapCacheWarmingEndpoints(this IEndpointRouteBuilder app) {
        RouteGroupBuilder route = app.MapGroup("/")
                                        .RequireAuthorization();

        route.MapGet("/websites/{id}/cachewarms", GetByWebsiteId);
        route.MapPost("/websites/{id}/cachewarms", StartCrawl);

        return route;
    }

    public static async Task<Ok<PagedResponse<CacheWarmDTO>>> GetByWebsiteId(Guid id, ICacheWarmingService cacheWarmingService, int page = 1, int perPage = 20) {
        PagedResponse<CacheWarmDTO> cacheWarms = await cacheWarmingService.GetCacheWarmsByWebsiteId(id, page, perPage);

        return TypedResults.Ok(cacheWarms);
    }

    public static async Task<ServerSentEventsResult<ProgressReport>> StartCrawl(Guid id, ICacheWarmingService cacheWarmingService, CancellationToken cancellationToken) {
        return TypedResults.ServerSentEvents(
                StreamCrawl(id, cacheWarmingService, cancellationToken),
                eventType: "crawl-update"
        );
    }

    private static async IAsyncEnumerable<ProgressReport> StreamCrawl(Guid id, ICacheWarmingService cacheWarmingSerivce, CancellationToken cancellationToken) {
        ThrottledProgress<CrawlProgress> progress = new(TimeSpan.FromSeconds(3));

        Task crawlTask = progress.RunAsync(async () => {
                await cacheWarmingSerivce.WarmCache(id, progress, cancellationToken);
        });

        await foreach(CrawlProgress update in progress.ReadAllAsync(cancellationToken)) {
            yield return new ProgressReport {
                LinksCrawled = update.TotalCrawled,
                LinksEnqueued = update.QueueSize
            };
        }

        await crawlTask;
    }
}
