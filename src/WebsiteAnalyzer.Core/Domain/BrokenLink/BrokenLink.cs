using System.Net;

namespace WebsiteAnalyzer.Core.Domain.BrokenLink;

public record BrokenLink
{
    public BrokenLink(BrokenLinkCrawl? brokenLinkCrawl, string targetPage, string referringPage, string anchorText,
        int line, HttpStatusCode statusCode)
    {
        Id = Guid.NewGuid();
        BrokenLinkCrawlId = brokenLinkCrawl?.Id;
        BrokenLinkCrawl = brokenLinkCrawl;
        TargetPage = targetPage;
        ReferringPage = referringPage;
        AnchorText = anchorText;
        Line = line;
        StatusCode = statusCode;
    }

    public BrokenLink()
    {
    }

    public Guid Id { get; set; }
    public Guid? BrokenLinkCrawlId { get; set; }
    public BrokenLinkCrawl? BrokenLinkCrawl { get; set; }
    public string TargetPage { get; set; }
    public string ReferringPage { get; set; }
    public string AnchorText { get; set; }
    public int Line { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}