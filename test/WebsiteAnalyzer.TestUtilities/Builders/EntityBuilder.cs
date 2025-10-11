using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.TestUtilities.Builders;

public abstract class EntityBuilder<T> where T : class
{
    protected readonly IBaseRepository<T> Repository;
    protected T Entity;

    protected EntityBuilder(IBaseRepository<T> repository)
    {
        Repository = repository;
    }
    
    public async Task<T> BuildAndSave()
    {
        await Repository.AddAsync(Entity);
        return Entity;
    }

    public T Build()
    {
        return Entity;
    }
}