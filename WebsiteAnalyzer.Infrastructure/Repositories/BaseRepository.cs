using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Exceptions;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public BaseRepository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .FindAsync(id)
            .ConfigureAwait(false);
    }

    public async Task<ICollection<T>> GetAllAsync()
    {
        return await DbSet
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task AddAsync(T entity)
    {
        try 
        {
            await DbSet.AddAsync(entity);
            await DbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new AlreadyExistsException($"Entity already exists");
        }
    }

    public async Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        await DbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        await DbContext.SaveChangesAsync()
            .ConfigureAwait(false);
    }
}