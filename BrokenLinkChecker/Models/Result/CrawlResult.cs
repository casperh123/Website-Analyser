using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.models.Result;

public record CrawlProgress<T> where T : Link
{
    public CrawlProgress(T link, int linksChecked, long linksEnqueued)
    {
        Link = link;
        LinksChecked = linksChecked;
        LinksEnqueued = (int)linksEnqueued;
    }

    public T Link { get; set; }
    public int LinksChecked { get; init; }
    public int LinksEnqueued { get; init; }
}