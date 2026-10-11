
using System.Security.Claims;
using Crawl.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyser.Api.DTOs;
using WebsiteAnalyser.Api.Streaming;
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

    public static async Task<ServerSentEventsResult<ProgressReport>> StartCrawl(IBrokenLinkService brokenLinkService, Guid id, CancellationToken cancellationToken) {
        return TypedResults.ServerSentEvents(
            StreamCrawl(id, brokenLinkService, cancellationToken),
            eventType: "crawl-update"
        );
    }

    private static async IAsyncEnumerable<ProgressReport> StreamCrawl(Guid id, IBrokenLinkService brokenLinkService, CancellationToken cancellationToken) {
        ThrottledProgress<CrawlProgress> progress = new(TimeSpan.FromSeconds(5));

        Task crawlTask = progress.RunAsync(async () => {
            await brokenLinkService.FindBrokenLinks(id, progress, cancellationToken);
        });

        await foreach(CrawlProgress update in progress.ReadAllAsync(cancellationToken)){
        
            yield return new ProgressReport {
                LinksCrawled = update.TotalCrawled,
                LinksEnqueued = update.QueueSize
            };
        }

        await crawlTask;

    }
}
