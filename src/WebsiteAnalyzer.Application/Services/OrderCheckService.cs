using WebsiteAnalyzer.Core.Domain.OrderChecks;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class OrderCheckService : IOrderCheckService
{
    private IOrderCheckRepository _orderCheckRepository;
    private IOrderCheckKeysRepository _keyRepository;

    public OrderCheckService(IOrderCheckRepository orderCheckRepository, IOrderCheckKeysRepository keyRepository)
    {
        _orderCheckRepository = orderCheckRepository;
        _keyRepository = keyRepository;
    }
    
    public Task<OrderCheck> GetOrderCheckById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ICollection<OrderCheck>> GetOrderChecksByWebsiteId(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderCheckKeys?> GetKeysByWebsiteId(Guid websiteId)
    {
        return await _keyRepository.GetByIdAsync(websiteId);
    }

    public async Task<OrderCheckKeys> UpdateKeys(OrderCheckKeys keys)
    {
        OrderCheckKeys? key = await _keyRepository.GetByIdAsync(keys.WebsiteId);

        if (key is null)
        {
            await _keyRepository.AddAsync(keys);

            return keys;
        }

        await _keyRepository.UpdateAsync(keys);

        return keys;
    }
}