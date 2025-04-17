using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly IDbContextFactory<ApplicationDbContext> DbContextFactory;

    public BaseRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        DbContextFactory = dbContextFactory;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();
        
        return await dbContext.Set<T>()
            .FindAsync(id);
    }

    public async Task<ICollection<T>> GetAllAsync()
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();
        
        return await dbContext.Set<T>()
            .ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();
        
        await dbContext.Set<T>()
            .AddAsync(entity);
        
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();
        
        dbContext.Set<T>()
            .Update(entity);
        
        await dbContext
            .SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();
   
        dbContext.Set<T>()
            .Remove(entity);
        
        await dbContext
            .SaveChangesAsync();
    }
}