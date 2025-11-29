using Crawler.Core;
using Crawler.Filters;
using Crawler.Models;
using Crawler.Visitors.BrokenLink;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class BrokenLinkService(
    HttpClient httpClient,
    IBrokenLinkCrawlRepository crawlRepository,
    IBrokenLinkRepository brokenLinkRepository)
    : IBrokenLinkService
{
    public async Task<ICollection<BrokenLinkDTO>> FindBrokenLinks(
        Website website, 
        IProgress<CrawlProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        BrokenLinkCrawl crawl = await CreateCrawlEntity(website.Url, website.UserId);
        ICollection<BrokenLinkDTO> brokenLinks = [];
        BrokenLinkVisitor brokenLinkVisitor = new BrokenLinkVisitor();
        int totalCrawled = 0;

        Progress<CrawlProgress> trackingProgress = new Progress<CrawlProgress>(p =>
        {
            totalCrawled = p.TotalCrawled;
            progress?.Report(p);
        });

        Crawler.Core.Crawler crawler = new CrawlerBuilder(httpClient)
            .WithFilter(new SameHostFilter())
            .WithVisitor(brokenLinkVisitor)
            .Build();

        try
        {
            await crawler.CrawlWebsiteAsync(new Uri(website.Url), trackingProgress, cancellationToken);
        
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

    public async Task<ICollection<BrokenLinkCrawlDTO>> GetCrawlsByUserAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            return [];
        }
        
        ICollection<BrokenLinkCrawl> brokenLinkCrawls = await crawlRepository.GetByUserAsync(userId) ?? [];

        return brokenLinkCrawls.Select(BrokenLinkCrawlDTO.From).ToList();
    }

    public async Task<ICollection<BrokenLinkDTO>> GetBrokenLinksByCrawlIdAsync(Guid crawlId)
    {
        IEnumerable<BrokenLink> brokenLinks = await brokenLinkRepository.GetBrokenLinksByCrawlAsync(crawlId);
        
        return brokenLinks
            .Select(BrokenLinkDTO.FromBrokenLink)
            .ToList();
    }

    public async Task<ICollection<BrokenLinkCrawlDTO>> GetBrokenLinkCrawlsByUrlAndUserId(string url, Guid userId)
    {
        IEnumerable<BrokenLinkCrawl> crawls = await crawlRepository.GetByUrlUserId(url, userId);

        return crawls
            .Select(BrokenLinkCrawlDTO.From)
            .ToList();
    }

    private async Task<BrokenLink> SaveBrokenLinkAsync(BrokenLinkCrawl? crawl, BrokenLinkReport report, LinkReference reference)
    {
        BrokenLink brokenLink = new BrokenLink(
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

    private async Task<BrokenLinkCrawl> CreateCrawlEntity(string url, Guid userId)
    {
        BrokenLinkCrawl crawl = new BrokenLinkCrawl(userId, url);
        await crawlRepository.AddAsync(crawl);

        return crawl;
    }
}