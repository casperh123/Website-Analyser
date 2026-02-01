using WebsiteAnalyzer.Core.Domain.OrderChecks;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class OrderCheckKeysRepository : BaseRepository<OrderCheckKeys>, IOrderCheckKeysRepository
{
    public OrderCheckKeysRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

}