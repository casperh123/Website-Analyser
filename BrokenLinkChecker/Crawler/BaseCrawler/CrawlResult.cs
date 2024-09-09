using System.Collections.Concurrent;
using System.Net;
using BrokenLinkChecker.models;
using BrokenLinkChecker.Models.Links;

namespace BrokenLinkChecker.crawler
{
    public class CrawlResult
    {
        public int LinksChecked { get; private set; }
        public int LinksEnqueued { get; private set; }

        public event Action<BrokenLink> OnBrokenLinks;
        public event Action<PageStat> OnPageVisited;
        public event Action<int> OnLinksEnqueued;
        public event Action<int> OnLinksChecked;

        public void AddResource(Link url, HttpResponseMessage response, long requestTime, long parseTime)
        {
            PageStat pageStat = new PageStat(url.Target, response, url.Type, requestTime, parseTime);
            
            OnPageVisited.Invoke(pageStat);

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
            OnBrokenLinks.Invoke(brokenLink);
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