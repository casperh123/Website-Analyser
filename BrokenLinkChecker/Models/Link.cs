namespace BrokenLinkChecker.models;

public class Link
{
    public string Referrer { get; set; }
    public string Target { get; set; }
    public string AnchorText { get; set; }
    public int Line { get; set; }

    public Link(string referrer, string target, string anchorText, int line)
    {
        Referrer = referrer ?? "";
        Target = target ?? "";
        AnchorText = anchorText ?? "";
        Line = line;
    }
}