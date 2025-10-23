using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.TestUtilities.Builders.Uptime;

public class DowntimePingBuilder : EntityBuilder<DowntimePing>
{


    public DowntimePingBuilder(IDowntimePingRepository repository) : base(repository)
    {
        Entity = new DowntimePing(
            Guid.NewGuid()
        );
    }

    public DowntimePingBuilder WithWebsiteId(Guid websiteId)
    {
        Entity.WebsiteId = websiteId;

        return this;
    }

    public DowntimePingBuilder WithTimeRecorded(DateTime timeRecorded)
    {
        Entity.TimeRecorded = timeRecorded;

        return this;
    }
}