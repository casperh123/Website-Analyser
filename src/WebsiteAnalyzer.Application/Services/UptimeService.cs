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

    public async Task<ICollection<DowntimePing>> GetDowntimePingsByWebsiteId(Guid websiteId)
    {
        return await _pingRepository.GetByWebsiteId(websiteId);
    }

    public async Task<DowntimePing> Ping(Website website)
    {
        DowntimePing ping = new DowntimePing(website.Id);

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