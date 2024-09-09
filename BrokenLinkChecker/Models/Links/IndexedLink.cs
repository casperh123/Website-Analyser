using System.Net;
using BrokenLinkChecker.models;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Models.Links;

public class IndexedLink : NavigationLink
{
    public string ReferringPage { get; }
    public string AnchorText { get; }
    public int Line { get; }
    public int StatusCode { get; }

    public IndexedLink(Link url, HttpStatusCode statuscode) : base(url.Target)
    {
        ReferringPage = url.Referrer;
        AnchorText = url.AnchorText;
        Line = url.Line;
        StatusCode = (int)statuscode;
    }

    public override string ToString()
    {
        return $"Broken Link Found: TARGET={Target}, ANCHOR TEXT='{AnchorText}', REFERRER={ReferringPage}, LINE={Line}, STATUS CODE={StatusCode}";
    }
}