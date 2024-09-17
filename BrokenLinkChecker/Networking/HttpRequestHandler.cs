using BrokenLinkChecker.crawler;
using BrokenLinkChecker.Models.Links;

namespace BrokenLinkChecker.Networking;

public class HttpRequestHandler(HttpClient httpClient, CrawlerConfig crawlerConfig)
{
    private HttpClient _httpClient = httpClient;
    private CrawlerConfig _crawlerConfig = crawlerConfig;
    
    public async Task<HttpResponseMessage> RequestPageAsync(Link url)
    {
        await _crawlerConfig.ApplyJitterAsync();
        return await _httpClient.GetAsync(url.target, HttpCompletionOption.ResponseHeadersRead);
    }
}