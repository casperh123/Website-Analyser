using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.TestUtilities.Builders;

public abstract class EntityBuilder<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected T Entity;

    protected EntityBuilder(ApplicationDbContext context)
    {
        Context = context;
    }
    
    public async Task<T> BuildAndSave()
    {
        Context.Set<T>().Add(Entity);
        await Context.SaveChangesAsync();
        return Entity;
    }

    public T Build()
    {
        return Entity;
    }
}