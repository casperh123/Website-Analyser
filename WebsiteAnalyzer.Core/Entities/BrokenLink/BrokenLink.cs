using System.Net;

namespace WebsiteAnalyzer.Core.Entities.BrokenLink;

public record BrokenLink
{
    public BrokenLink(Guid id, Guid brokenLinkCrawlId, BrokenLinkCrawl brokenLinkCrawl, string targetPage, string referringPage, string anchorText, int line, HttpStatusCode statusCode)
    {
        Id = id;
        BrokenLinkCrawlId = brokenLinkCrawlId;
        BrokenLinkCrawl = brokenLinkCrawl;
        TargetPage = targetPage;
        ReferringPage = referringPage;
        AnchorText = anchorText;
        Line = line;
        StatusCode = statusCode;
    }

    public Guid Id { get; set; }
    public required Guid BrokenLinkCrawlId { get; set; }
    public required BrokenLinkCrawl BrokenLinkCrawl { get; set; }
    public required string TargetPage { get; set; }
    public required string ReferringPage { get; set; }
    public required string AnchorText { get; set; }
    public required int Line { get; set; }
    public required HttpStatusCode StatusCode { get; set; }
    
    
}