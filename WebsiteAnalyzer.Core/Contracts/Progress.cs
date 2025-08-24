namespace WebsiteAnalyzer.Core.Contracts;

public class Progress
{
    public int LinksEnqueued { get; init; }
    public int LinksChecked { get; init; }

    public Progress(int linksEnqueued, int linksChecked)
    {
        LinksEnqueued = linksEnqueued;
        LinksChecked = linksChecked;
    }
}