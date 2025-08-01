namespace WebsiteAnalyzer.Core.Contracts;

public class Progress
{
    private int LinksEnqueued { get; init; }
    private int LinksChecked { get; init; }

    public Progress(int linksEnqueued, int linksChecked)
    {
        LinksEnqueued = linksEnqueued;
        LinksChecked = linksChecked;
    }
}