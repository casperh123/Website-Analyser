using BrokenLinkChecker.DocumentParsing.Linkextraction;
using BrokenLinkChecker.models;

namespace BrokenLinkChecker.crawler;

public class LinkProcessor(CrawlerConfig crawlerConfig)
{
    private LinkExtractor _linkExtractor = new LinkExtractor(crawlerConfig);
        
    public async Task<IEnumerable<Link>> GetLinksFromResponse(HttpResponseMessage response, Link url)
    {
        if(!response.IsSuccessStatusCode)
        {
            return [];
        }
    
        List<Link> links = await _linkExtractor.GetLinksFromResponseAsync(response, url);
        
        return links;
    }
}