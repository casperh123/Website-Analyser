namespace BrokenLinkChecker.models;

public class BrokenLink
{
    public string Url { get; set; }
    public string ReferringPage { get; set; }
    public string AnchorText { get; set; }
    public int Line { get; set; }
    public int StatusCode { get; set; }

    public BrokenLink(string url, string referringPage, string anchorText, int line, int statusCode, string errorMessage = "")
    {
        Url = url;
        ReferringPage = referringPage;
        AnchorText = anchorText;
        Line = line;
        StatusCode = statusCode;
    }

    public override string ToString()
    {
        return $"Broken Link Found: TARGET={Url}, ANCHOR TEXT='{AnchorText}', REFERRER={ReferringPage}, LINE={Line}, STATUS CODE={StatusCode}";
    }
}