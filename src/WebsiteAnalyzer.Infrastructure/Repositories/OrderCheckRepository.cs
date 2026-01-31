using WebsiteAnalyzer.Core.Domain.OrderChecks;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class OrderCheckRepository : BaseRepository<OrderCheck>, IOrderCheckRepository
{
    public OrderCheckRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}