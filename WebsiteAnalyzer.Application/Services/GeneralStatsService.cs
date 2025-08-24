using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class GeneralStatsService : IGeneralStatsService
{
    private HttpClient _httpClient;
    
    public GeneralStatsService(HttpClient httpClient)
    {
        _httpClient = _httpClient;
    }
    
    public async Task<GeneralStats> GetGeneralStatsForWebsite(Website website)
    {
        HttpResponseMessage request = await _httpClient.GetAsync(website.Url);
        
        throw new NotImplementedException();
    }
}