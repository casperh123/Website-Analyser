using System.Net;
using BrokenLinkChecker.models;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Models.Links;

public class BrokenLink(Link url, HttpStatusCode statuscode) : NavigationLink(url.Target)
{
    public string ReferringPage { get; } = url.Referrer;
    public string AnchorText { get; } = url.AnchorText;
    public int Line { get; } = url.Line;
    public int StatusCode { get; } = (int)statuscode;

    public override string ToString()
    {
        return $"Broken Link Found: TARGET={Target}, ANCHOR TEXT='{AnchorText}', REFERRER={ReferringPage}, LINE={Line}, STATUS CODE={StatusCode}";
    }
}