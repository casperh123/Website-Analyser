using BrokenLinkChecker.Crawler.BaseCrawler;
using BrokenLinkChecker.Models.Links;

namespace BrokenLinkChecker.Networking;

public class HttpRequestHandler(HttpClient httpClient, CrawlerConfig crawlerConfig)
{
    private readonly CrawlerConfig _crawlerConfig = crawlerConfig;
    private readonly HttpClient _httpClient = httpClient;

    public async Task<HttpResponseMessage> RequestPageAsync(TraceableLink url)
    {
        await _crawlerConfig.ApplyJitterAsync();
        return await _httpClient.GetAsync(url.Target, HttpCompletionOption.ResponseHeadersRead);
    }
}