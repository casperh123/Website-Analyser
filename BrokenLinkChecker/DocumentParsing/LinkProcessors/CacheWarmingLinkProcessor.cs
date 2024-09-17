using System.Net;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;
using BrokenLinkChecker.models.Links;
using BrokenLinkChecker.Models.Links;

namespace BrokenLinkChecker.DocumentParsing.LinkProcessors;

public class CacheWarmingLinkProcessor : ILinkProcessor<TargetLink>
{
    private readonly ISet<TargetLink> _visitedResources;
    private readonly HttpClient _httpClient;
    private readonly AbstractLinkExtractor<TargetLink> _linkExtractor; 
    
    public CacheWarmingLinkProcessor(HttpClient httpClient)
    {
        _visitedResources = new HashSet<TargetLink>();
        _httpClient = httpClient;
        _linkExtractor = new NavigationLinkExtractor();
    } 
    
    public async Task<IEnumerable<TargetLink>> ProcessLinkAsync(TargetLink link, ModularCrawlResult<TargetLink> crawlResult)
    {
        IEnumerable<TargetLink> links = Enumerable.Empty<TargetLink>();

        if (!_visitedResources.Contains(link))
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(link.Target, HttpCompletionOption.ResponseHeadersRead);
                
                links = await _linkExtractor.GetLinksFromDocument(response, link);
                
                _visitedResources.Add(link);
            }
            catch (Exception e)
            {
                // Just continue the cache warming.
            }

            crawlResult.IncrementLinksChecked();
        }

        return links;
    }

}