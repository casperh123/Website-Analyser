using WebsiteAnalyzer.Core.Domain.OrderChecks;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IOrderCheckService
{
    Task<OrderCheck?> CheckOrder(Guid websiteId);
    Task<OrderCheck?> GetOrderCheckById(Guid id);
    Task<OrderCheck?> GetLatestByWebsiteId(Guid websiteId);
    Task<OrderCheckKeys?> GetKeysByWebsiteId(Guid websiteId);
    Task<OrderCheckKeys> UpdateKeys(OrderCheckKeys keys);
}