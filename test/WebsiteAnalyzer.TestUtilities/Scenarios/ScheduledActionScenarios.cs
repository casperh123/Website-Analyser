using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Infrastructure.Data;
using Xunit.Sdk;

namespace WebsiteAnalyzer.TestUtilities.Scenarios;

public class ScheduledActionScenarios
{
    private readonly ApplicationDbContext _dbContext;

    public ScheduledActionScenarios(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ScheduledAction> DefaultScheduledAction()
    {
        throw new NotImplementedException();
    }
}