namespace WebsiteAnalyzer.Core.Contracts;

public class Progress
{
    public Progress(int linksEnqueued, int linksChecked)
    {
        LinksEnqueued = linksEnqueued;
        LinksChecked = linksChecked;
    }

    public int LinksEnqueued { get; init; }
    public int LinksChecked { get; init; }
}