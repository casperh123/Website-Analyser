using WebsiteAnalyzer.Core.Domain.OrderChecks;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace WebsiteAnalyzer.Application.Services;

public class OrderCheckService : IOrderCheckService
{
    private IOrderCheckRepository _orderCheckRepository;
    private IOrderCheckKeysRepository _keyRepository;
    private IWebsiteRepository _websiteRepository;

    public OrderCheckService(IOrderCheckRepository orderCheckRepository, IOrderCheckKeysRepository keyRepository, IWebsiteRepository websiteRepository)
    {
        _orderCheckRepository = orderCheckRepository;
        _keyRepository = keyRepository;
        _websiteRepository = websiteRepository;
    }

    public async Task<OrderCheck?> CheckOrder(Guid websiteId)
    {
        Website website = await _websiteRepository.GetByWebsiteId(websiteId);
        OrderCheck check = new OrderCheck();
        OrderCheckKeys keys = await _keyRepository.GetByIdAsync(websiteId);

        if (keys is null)
        {
            return null;
        }

        RestAPI api = new RestAPI(website?.Url + "/wp-json/wc/v3", keys.Key, keys.Secret);
        WCObject wc = new WCObject(api);

        List<Order>? orders = await wc.Order.GetAll();

        Order latest = orders.First();

        check.TimeSinceLastOrder = DateTime.Now.Subtract(latest.date_created.Value);

        await _orderCheckRepository.AddAsync(check);

        return check;

    }
    
    public Task<OrderCheck?> GetOrderCheckById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderCheck?> GetLatestByWebsiteId(Guid websiteId)
    {
        return await _orderCheckRepository.GetLatestByWebsiteId(websiteId);
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