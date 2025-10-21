using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class UptimeService : IUptimeService
{
    private readonly IDownTimePingRepository _pingRepository;
    private readonly HttpClient _httpClient;

    public UptimeService(IDownTimePingRepository pingRepository, HttpClient httpClient)
    {
        _pingRepository = pingRepository;
        _httpClient = httpClient;
    }

    public async Task<DownTimePing> Ping(Website website)
    {
        DownTimePing ping = new DownTimePing(website.Id);

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(website.Url);

            if (response.IsSuccessStatusCode)
            {
                return ping;
            }
            
            ping.StatusCode = response.StatusCode;
            ping.Reason = response.ReasonPhrase;
            
            await _pingRepository.AddAsync(ping);
        }
        catch (HttpRequestException requestException)
        {
            ping.StatusCode = requestException.StatusCode;
            ping.Reason = requestException.Message;
         
            await _pingRepository.AddAsync(ping);
        }

        return ping;
    }
}