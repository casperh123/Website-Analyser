using WebsiteAnalyzer.Core.Domain.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IWebsiteRepository : IBaseRepository<Website>
{
    Task<ICollection<Website>> GetAllByUserId(Guid userId);
    Task<Website?> GetByIdAndUserId(Guid id, Guid usersId);
    Task DeleteByUrlAndUserId(string url, Guid userId);
}