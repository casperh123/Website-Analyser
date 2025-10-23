using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Builders.Uptime;

namespace WebsiteAnalyzer.TestUtilities.Scenarios;

public class DowntimePingScenarios
{
    private readonly IDowntimePingRepository _pingRepository;

    public DowntimePingScenarios(ApplicationDbContext context)
    {
        _pingRepository = new DowntimePingRepository(context);
    }

    public async Task<DowntimePing> Default()
    {
        return await new DowntimePingBuilder(_pingRepository)
            .BuildAndSave();
    }

    public async Task<DowntimePing> WithWebsiteId(Guid websiteId)
    {
        return await new DowntimePingBuilder(_pingRepository)
            .WithWebsiteId(websiteId)
            .BuildAndSave();
    }

    public async Task<DowntimePing> WithWebsiteIdAndTimeRecorded(Guid websiteId, DateTime timeRecorded)
    {
        return await new DowntimePingBuilder(_pingRepository)
            .WithWebsiteId(websiteId)
            .WithTimeRecorded(timeRecorded)
            .BuildAndSave();
    }

    public async Task<ICollection<DowntimePing>> MultipleWithWebsiteId(int amount, Guid websiteId)
    {
        ICollection<DowntimePing> pings = [];

        for (int i = 0; i < amount; i++)
        {
            DowntimePing ping = await new DowntimePingBuilder(_pingRepository)
                .WithWebsiteId(websiteId)
                .BuildAndSave();
            
            pings.Add(ping);
        }

        return pings;
    }
}