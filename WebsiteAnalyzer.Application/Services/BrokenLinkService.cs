using System.Net;
using System.Runtime.CompilerServices;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.models.Result;
using WebsiteAnalyzer.Core.DataTransferObject;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Events;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

public class BrokenLinkService : IBrokenLinkService
{
    private readonly HttpClient _httpClient;
    private readonly IBrokenLinkCrawlRepository _crawlRepository;
    private readonly IBrokenLinkRepository _brokenLinkRepository;

    public event EventHandler<CrawlProgressEventArgs>? ProgressUpdated;

    public BrokenLinkService(
        HttpClient httpClient,
        IBrokenLinkCrawlRepository crawlRepository,
        IBrokenLinkRepository brokenLinkRepository)
    {
        _httpClient = httpClient;
        _crawlRepository = crawlRepository;
        _brokenLinkRepository = brokenLinkRepository;
    }

    public async IAsyncEnumerable<BrokenLinkDTO> FindBrokenLinks(
        string url,
        Guid? userId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {

        BrokenLinkProcessor linkProcessor = new BrokenLinkProcessor(_httpClient);
        ModularCrawler<IndexedLink> crawler = new ModularCrawler<IndexedLink>(linkProcessor);
        IndexedLink startLink = new IndexedLink(string.Empty, url, "", 0);
        IAsyncEnumerable<CrawlProgress<IndexedLink>> crawlProgress = crawler.CrawlWebsiteAsync(startLink, cancellationToken);
        BrokenLinkCrawl? crawl = null;

        if (userId.HasValue)
        {
            crawl = await SaveBrokenLinkCrawl(url, userId.Value);
        }

        await foreach (CrawlProgress<IndexedLink> progress in crawlProgress)
        {
            IndexedLink link = progress.Link;
            UpdateProgress(progress);
            
            
            if (link.StatusCode == HttpStatusCode.OK)
            {
                continue;
            }

            if (userId.HasValue && crawl is not null)
            {
                await SaveBrokenLinkAsync(
                    crawl,
                    link
                );
            }

            yield return BrokenLinkDTO.FromIndexedLink(link);
        }
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

        await _brokenLinkRepository.AddAsync(brokenLink);
    }
    

    private async Task<BrokenLinkCrawl> SaveBrokenLinkCrawl(string url, Guid userId)
    {
        BrokenLinkCrawl crawl = new BrokenLinkCrawl(userId, url);
        await _crawlRepository.AddAsync(crawl);
        return crawl;
    }

    private void UpdateProgress(CrawlProgress<IndexedLink> progress)
    {
        ProgressUpdated?.Invoke(
            this, 
            new CrawlProgressEventArgs(
                progress.LinksEnqueued,
                progress.LinksChecked
                )
            );
    }
}