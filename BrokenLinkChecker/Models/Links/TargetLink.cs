namespace BrokenLinkChecker.models.Links;

public record TargetLink(string target)
{
    public string Target { get; } = target;
}