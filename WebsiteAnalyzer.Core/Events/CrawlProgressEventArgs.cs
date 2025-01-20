namespace WebsiteAnalyzer.Core.Events;

public class CrawlProgressEventArgs : EventArgs
{
    public int LinksEnqueued { get; }
    public int LinksChecked { get; }
    
    public CrawlProgressEventArgs(int linksEnqueued, int linksChecked)
    {
        LinksChecked = linksChecked;
        LinksEnqueued = linksEnqueued;
    }
}