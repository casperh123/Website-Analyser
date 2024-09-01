using System.Net;

namespace BrokenLinkChecker.models;

public class BrokenLink
{
    public string Url { get; }
    public string ReferringPage { get; }
    public string AnchorText { get; }
    public int Line { get; }
    public int StatusCode { get; }

    public BrokenLink(Link url, HttpStatusCode statuscode)
    {
        Url = url.Target;
        ReferringPage = url.Referrer;
        AnchorText = url.AnchorText;
        Line = url.Line;
        StatusCode = (int)statuscode;
    }

    public override string ToString()
    {
        return $"Broken Link Found: TARGET={Url}, ANCHOR TEXT='{AnchorText}', REFERRER={ReferringPage}, LINE={Line}, STATUS CODE={StatusCode}";
    }
}