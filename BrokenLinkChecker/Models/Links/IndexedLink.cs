using System.Net;
using BrokenLinkChecker.models;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Models.Links;

public class IndexedLink : Link
{
    public string ReferringPage { get; set; }
    public string AnchorText { get; set; }
    public int Line { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    public IndexedLink(TraceableLink url, HttpStatusCode statuscode) : base(url.Target)
    {
        ReferringPage = url.Referrer;
        AnchorText = url.AnchorText;
        Line = url.Line;
        StatusCode = statuscode;
    }
    
    public IndexedLink(string referrer, string target, string anchorText, int line, HttpStatusCode statuscode = HttpStatusCode.Unused) : base(target)
    {
        ReferringPage = referrer;
        AnchorText = anchorText;
        Line = line;
        StatusCode = statuscode;
    }

    public override string ToString()
    {
        return $"Broken Link Found: TARGET={Target}, ANCHOR TEXT='{AnchorText}', REFERRER={ReferringPage}, LINE={Line}, STATUS CODE={StatusCode}";
    }
}