using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.TestUtilities.Builders;

public class ScheduledActionBuilder : EntityBuilder<ScheduledAction>
{


    public ScheduledActionBuilder(
        IScheduledActionRepository repository,
        Core.Domain.Website.Website website,
        Frequency frequency,
        CrawlAction action,
        TimeSpan offset = default
        ) : base(repository)
    {
        Entity = new ScheduledAction(
            website,
            frequency,
            action,
            offset
        );
    }
    
    
}