using System.Net;
using System.Runtime.CompilerServices;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.Models.Result;
using WebsiteAnalyzer.Core.Contracts;
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
    public async IAsyncEnumerable<BrokenLinkDTO> FindBrokenLinks(
        Website website, 
        IProgress<Progress>? progress = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        BrokenLinkCrawl? crawl = await CreateCrawlEntity(website.Url, website.UserId);
    
        try
        {
            await foreach (BrokenLinkDTO brokenLink in StreamBrokenLinksInternal(crawl, website.Url, progress, cancellationToken))
            {
                yield return brokenLink;
            }
        }
        finally
        {
            await SaveBrokenLinkCrawl(crawl);
        }
    }
    
    public async IAsyncEnumerable<BrokenLinkDTO> FindBrokenLinksAnonymus(
        string url, IProgress<Progress>? progress = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<BrokenLinkDTO> brokenLinks = StreamBrokenLinksInternal(null, url, progress, cancellationToken);

        await foreach (BrokenLinkDTO brokenLink in brokenLinks)
        {
            yield return brokenLink;
        }    
    }

    private async IAsyncEnumerable<BrokenLinkDTO> StreamBrokenLinksInternal(
        BrokenLinkCrawl? crawl,
        string url,
        IProgress<Progress>? progress,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<CrawlProgress<IndexedLink>> crawlProgress = StartCrawler(url, cancellationToken);
    
        await foreach (CrawlProgress<IndexedLink> step in crawlProgress)
        {
            IndexedLink link = step.Link;
        
            progress?.Report(new Progress(step.LinksEnqueued, step.LinksChecked));

            if (link.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.MovedPermanently))            {
                if (crawl is not null)
                {
                    await SaveBrokenLinkAsync(crawl, link);   
                }
                yield return BrokenLinkDTO.FromIndexedLink(link);
            }

            if (crawl is not null)
            {
                crawl.LinksChecked = step.LinksChecked;
            }
        }
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

    private async Task SaveBrokenLinkAsync(BrokenLinkCrawl crawl, IndexedLink link)
    {
        BrokenLink brokenLink = new BrokenLink(
            crawl,
            link.Target,
            link.ReferringPage,
            link.AnchorText,
            link.Line,
            link.StatusCode
        );

        await brokenLinkRepository.AddAsync(brokenLink);
    }
    
    private async Task SaveBrokenLinkCrawl(BrokenLinkCrawl crawl)
    {
        await crawlRepository.UpdateAsync(crawl);
    }

    private async Task<BrokenLinkCrawl?> GetByCrawlByIdAsync(Guid? crawlId)
    {
        if (!crawlId.HasValue)
        {
            return null;
        }

        return await crawlRepository.GetByIdAsync(crawlId.Value);
    }
    
    private IAsyncEnumerable<CrawlProgress<IndexedLink>> StartCrawler(string url, CancellationToken cancellationToken)
    {
        BrokenLinkProcessor linkProcessor = new BrokenLinkProcessor(httpClient);
        ModularCrawler<IndexedLink> crawler = new ModularCrawler<IndexedLink>(linkProcessor);
        IndexedLink startLink = new IndexedLink(url);
        
        return crawler.CrawlWebsiteAsync(startLink, cancellationToken);
    }

    private async Task<BrokenLinkCrawl> CreateCrawlEntity(string url, Guid userId)
    {
        BrokenLinkCrawl crawl = new BrokenLinkCrawl(userId, url);
        await crawlRepository.AddAsync(crawl);

        return crawl;
    }
}