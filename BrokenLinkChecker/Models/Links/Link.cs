using BrokenLinkChecker.models;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Models.Links;

public class Link(string referrer, string target, string anchorText, int line, ResourceType type) : NavigationLink(target)
{
    public string Referrer = referrer;    
    public string AnchorText { get; } = anchorText;
    public int Line { get; } = line;
    public ResourceType Type { get; } = type;
}