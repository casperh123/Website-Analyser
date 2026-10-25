using Crawl.Core;
using Crawl.Core.Builders;
using Crawl.Filters;
using Crawl.Models;
using Crawl.Visitors.BrokenLink;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Exceptions;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.BrokenLink;

namespace WebsiteAnalyzer.Application.Services;

public class BrokenLinkService(
    HttpClient httpClient,
    IBrokenLinkCrawlRepository crawlRepository,
    IBrokenLinkRepository brokenLinkRepository,
    IWebsiteRepository websiteRepository)
    : IBrokenLinkService
{
    public async Task<ICollection<BrokenLinkDTO>> FindBrokenLinks(
        Guid websiteId, 
        IProgress<CrawlProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Website website = await websiteRepository.GetByWebsiteId(websiteId)
            ?? throw new NotFoundException($"Website Id: {websiteId}");
        BrokenLinkCrawl crawl = await CreateCrawlEntity(website);
        ICollection<BrokenLinkDTO> brokenLinks = [];
        BrokenLinkVisitor brokenLinkVisitor = new();
        int totalCrawled = 0;

        Progress<CrawlProgress> trackingProgress = new(p =>
        {
            totalCrawled = p.TotalCrawled;
            progress?.Report(p);
        });

        Crawler crawler = new SequentialCrawlerBuilder(httpClient)
            .WithFilter(new SameHostFilter())
            .WithVisitor(brokenLinkVisitor)
            .Build();

        try
        {
            await crawler.CrawlAsync(new Uri(website.Url), trackingProgress, cancellationToken);
        
            foreach (BrokenLinkReport brokenLinkReport in brokenLinkVisitor.GetBrokenLinks())
            {
                foreach (LinkReference reference in brokenLinkReport.References)
                {
                    BrokenLink brokenLink = await SaveBrokenLinkAsync(crawl, brokenLinkReport, reference);
                    brokenLinks.Add(BrokenLinkDTO.FromBrokenLink(brokenLink));
                }
            }
        }
        finally
        {
            crawl.LinksChecked = totalCrawled;
            await SaveBrokenLinkCrawl(crawl);
        }

        return brokenLinks;
    }

    public async Task<ICollection<BrokenLinkDTO>> GetBrokenLinksByCrawlIdAsync(Guid crawlId)
    {
        IEnumerable<BrokenLink> brokenLinks = await brokenLinkRepository.GetBrokenLinksByCrawlAsync(crawlId);
        
        return [.. brokenLinks.Select(BrokenLinkDTO.FromBrokenLink)];
    }

    public async Task<PagedResponse<BrokenLinkCrawlDTO>> GetCrawlsByWebsiteId(Guid id, int page = 1, int perPage = 20) {
        PagedResponse<BrokenLinkCrawl> items = await crawlRepository.GetByWebsiteId(id, page, perPage);

        return items.Map(BrokenLinkCrawlDTO.From);
    }

    private async Task<BrokenLink> SaveBrokenLinkAsync(BrokenLinkCrawl? crawl, BrokenLinkReport report, LinkReference reference)
    {
        BrokenLink brokenLink = new(
            crawl,
            report.Url,
            reference.LinkedFrom,
            reference.AnchorText ?? "",
            reference.Line ?? -1,
            report.StatusCode
        );

        await brokenLinkRepository.AddAsync(brokenLink);
        
        return brokenLink;
    }
    
    private async Task SaveBrokenLinkCrawl(BrokenLinkCrawl crawl)
    {
        await crawlRepository.UpdateAsync(crawl);
    }

    private async Task<BrokenLinkCrawl> CreateCrawlEntity(Website website)
    {
        BrokenLinkCrawl crawl = new(website);
        await crawlRepository.AddAsync(crawl);

        return crawl;
    }
}
