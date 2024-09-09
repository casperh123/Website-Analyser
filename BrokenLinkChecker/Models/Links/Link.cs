namespace BrokenLinkChecker.models;

public struct Link(string referrer, string target, string anchorText, int line, ResourceType type)
{
    public string Referrer { get; } = referrer;
    public string Target { get; } = target;
    public string AnchorText { get; } = anchorText;
    public int Line { get; } = line;
    public ResourceType Type { get; } = type;
}