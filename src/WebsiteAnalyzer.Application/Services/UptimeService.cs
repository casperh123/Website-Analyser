using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Uptime;

namespace WebsiteAnalyzer.Application.Services;

public class UptimeService : IUptimeService
{
    private readonly IDowntimePingRepository _pingRepository;
    private readonly IWebsiteRepository _websiteRepository;
    private readonly HttpClient _httpClient;

    public UptimeService(IDowntimePingRepository pingRepository, IWebsiteRepository websiteRepository, HttpClient httpClient)
    {
        _pingRepository = pingRepository;
        _websiteRepository = websiteRepository;
        _httpClient = httpClient;
    }

    public async Task<ICollection<UptimeStatusDTO>> GetByWebsiteAfterDate(Guid websiteId, DateTime afterDate)
    {
        ICollection<DowntimePing> pings = await _pingRepository.GetByWebsiteIdAfterDate(websiteId, afterDate);
        DateTime currentTime = DateTime.UtcNow;
        afterDate = TruncateToHour(afterDate);
    
        // Create a complete timeline of minute intervals
        List<DateTime> timeline = Enumerable.Range(0, (int)(currentTime - afterDate).TotalHours + 1)
            .Select(i => afterDate.AddHours(i))
            .ToList();
    
        // Group pings by minute and merge with timeline
        Dictionary<DateTime, List<DowntimePing>> pingsByHour = pings.GroupBy(p => TruncateToHour(p.TimeRecorded))
            .ToDictionary(g => g.Key, g => g.ToList());

        return timeline.Select(hour =>
        {
            pingsByHour.TryGetValue(hour, out List<DowntimePing>? downtimePings);
            DowntimePing? ping = downtimePings?.OrderByDescending(p => p.StatusCode).FirstOrDefault();

            DateTime timeRecorded = ping?.TimeRecorded ?? hour;
            
            return new UptimeStatusDTO(
                downtimePings?.Count ?? 0,
                TruncateToHour(timeRecorded),
                ping?.StatusCode,
                ping?.Reason
            );
        }).ToList();
    }

    public async Task<DowntimePing> Ping(Guid websiteId)
    {
        Website website = await _websiteRepository.GetByWebsiteId(websiteId);
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

    private DateTime TruncateToHour(DateTime time)
    {
        return new DateTime(
            time.Year, 
            time.Month, 
            time.Day, 
            time.Hour, 
            0,
            0
        );
    }
}