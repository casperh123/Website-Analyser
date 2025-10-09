namespace WebsiteAnalyzer.Core.Events;

public class CrawlProgressEventArgs : EventArgs
{
    public CrawlProgressEventArgs(int linksEnqueued, int linksChecked)
    {
        LinksChecked = linksChecked;
        LinksEnqueued = linksEnqueued;
    }

    public int LinksEnqueued { get; }
    public int LinksChecked { get; }
}