using WebsiteAnalyzer.Core.Domain.OrderChecks;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IOrderCheckRepository : IBaseRepository<OrderCheck>
{
    Task<OrderCheck?> GetLatestByWebsiteId(Guid websiteId);
}