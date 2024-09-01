using BrokenLinkChecker.models;

namespace BrokenLinkChecker.crawler
{
    public class CrawlResult
    {
        public ICollection<BrokenLink> BrokenLinks { get; } = new List<BrokenLink>();
        public ICollection<PageStats> VisitedPages { get; } = new List<PageStats>();
        public int LinksChecked { get; private set; }
        public int LinksEnqueued { get; private set; }

        public Action<ICollection<BrokenLink>> OnBrokenLinks { get; set; }
        public Action<ICollection<PageStats>> OnPageVisited { get; set; }
        public Action<int> OnLinksEnqueued { get; set; }
        public Action<int> OnLinksChecked { get; set; }

        public void AddBrokenLink(BrokenLink brokenLink)
        {
            BrokenLinks.Add(brokenLink);
            OnBrokenLinks?.Invoke(BrokenLinks);
        }

        public void AddVisitedPage(PageStats pageStats)
        {
            VisitedPages.Add(pageStats);
            OnPageVisited?.Invoke(VisitedPages);
        }

        public void IncrementLinksChecked()
        {
            LinksChecked++;
            OnLinksChecked?.Invoke(LinksChecked);
        }

        public void SetLinksEnqueued(int count)
        {
            LinksEnqueued = count;
            OnLinksEnqueued?.Invoke(LinksEnqueued);
        }
    }
}