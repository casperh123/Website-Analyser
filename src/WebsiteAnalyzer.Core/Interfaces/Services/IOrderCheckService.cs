using WebsiteAnalyzer.Core.Domain.OrderChecks;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IOrderCheckService
{
    Task<OrderCheck> GetOrderCheckById(Guid id);
    Task<ICollection<OrderCheck>> GetOrderChecksByWebsiteId(Guid websiteId);
    Task<OrderCheckKeys?> GetKeysByWebsiteId(Guid websiteId);
    Task<OrderCheckKeys> UpdateKeys(OrderCheckKeys keys);
}