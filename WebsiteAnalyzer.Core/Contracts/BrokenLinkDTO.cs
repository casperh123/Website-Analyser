using System.Net;
using BrokenLinkChecker.Models.Links;
using WebsiteAnalyzer.Core.Entities.BrokenLink;

namespace WebsiteAnalyzer.Core.Contracts;

public record BrokenLinkDTO
{
    public BrokenLinkDTO(string targetPage, string referringPage, string anchorText, int line, HttpStatusCode statusCode)
    {
        TargetPage = targetPage;
        ReferringPage = referringPage;
        AnchorText = anchorText;
        Line = line;
        StatusCode = statusCode;
    }
    
    public string TargetPage { get; set; }
    public string ReferringPage { get; set; }
    public string AnchorText { get; set; }
    public int Line { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    
    public string NormalizedAnchorText() => string.IsNullOrEmpty(AnchorText) ? "N/A" : AnchorText;

    public BrokenLink ToBrokenLink()
    {
        return new BrokenLink
        {
            TargetPage = TargetPage,
            ReferringPage = ReferringPage,
            AnchorText = AnchorText,
            Line = Line,
            StatusCode = StatusCode
        };
    }
    
    public static BrokenLinkDTO FromIndexedLink(IndexedLink link) => new BrokenLinkDTO(link.Target, link.ReferringPage, link.AnchorText, link.Line, link.StatusCode);
    public static BrokenLinkDTO FromBrokenLink(BrokenLink link) => new BrokenLinkDTO(link.TargetPage, link.ReferringPage, link.AnchorText, link.Line, link.StatusCode);
}