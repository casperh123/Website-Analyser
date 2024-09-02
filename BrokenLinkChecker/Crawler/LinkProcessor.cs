using BrokenLinkChecker.DocumentParsing.Linkextraction;
using BrokenLinkChecker.models;
using BrokenLinkChecker.Networking;
using BrokenLinkChecker.utility;

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
        
        return links.Where(link => !IsExcluded(link.Target));
    }
    
    private static bool IsExcluded(string url)
    {
        string[] excludedExtensions = { ".js", ".css" };

        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        {
            string path = uri.LocalPath;

            if (excludedExtensions.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        string[] asyncKeywords = { "ajax", "async", "action=async" };
        if (asyncKeywords.Any(keyword => url.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
            
        return url.Contains('#') || url.Contains('?') || url.Contains("mailto");
    }
}