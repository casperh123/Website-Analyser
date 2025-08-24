using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Entities.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IGeneralStatsService
{
    Task<GeneralStats> GetGeneralStatsForWebsite(Website website);
}