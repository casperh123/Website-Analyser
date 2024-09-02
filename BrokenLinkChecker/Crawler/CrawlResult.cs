using System.Net;
using BrokenLinkChecker.models;

namespace BrokenLinkChecker.crawler
{
    public class CrawlResult
    {
        public ICollection<BrokenLink> BrokenLinks { get; } = new List<BrokenLink>();
        public ICollection<PageStat> VisitedPages { get; } = new List<PageStat>();
        public int LinksChecked { get; private set; }
        public int LinksEnqueued { get; private set; }

        public Action<ICollection<BrokenLink>> OnBrokenLinks { get; set; }
        public Action<ICollection<PageStat>> OnPageVisited { get; set; }
        public Action<int> OnLinksEnqueued { get; set; }
        public Action<int> OnLinksChecked { get; set; }

        public void AddResource(Link url, HttpResponseMessage response, long requestTime, long parseTime)
        {
            VisitedPages.Add(new PageStat(url.Target, response, url.Type, requestTime, parseTime));
            OnPageVisited?.Invoke(VisitedPages);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                HandleBrokenLink(url, response.StatusCode);
            }
        }
        
        public void AddResource(Link url, HttpStatusCode statusCode) {
            if (statusCode == HttpStatusCode.NotFound)
            {
                HandleBrokenLink(url, statusCode);
            }
        }

        private void HandleBrokenLink(Link url, HttpStatusCode statusCode)
        {
            if (statusCode != HttpStatusCode.Forbidden)
            {
                AddBrokenLink(new BrokenLink(url, statusCode));
            }
        }
        
        private void AddBrokenLink(BrokenLink brokenLink)
        {
            BrokenLinks.Add(brokenLink);
            OnBrokenLinks?.Invoke(BrokenLinks);
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