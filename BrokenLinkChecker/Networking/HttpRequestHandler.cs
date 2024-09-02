using BrokenLinkChecker.crawler;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.Networking;

public class HttpRequestHandler(HttpClient httpClient, CrawlerConfig crawlerConfig)
{
    private HttpClient _httpClient = httpClient;
    private CrawlerConfig _crawlerConfig = crawlerConfig;
    
    public async Task<(HttpResponseMessage, long)> RequestPageWithBenchmarkAsync(Link url)
    {
        await _crawlerConfig.ApplyJitterAsync();
        return await Utilities.BenchmarkAsync(() => _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead));
    }
    
    public async Task<HttpResponseMessage> RequestPageAsync(Link url)
    {
        await _crawlerConfig.ApplyJitterAsync();
        return await _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead);
    }
}