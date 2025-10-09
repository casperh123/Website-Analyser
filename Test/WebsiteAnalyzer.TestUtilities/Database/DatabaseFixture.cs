using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.TestUtilities.Database;

public class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public DbContextOptions<ApplicationDbContext> Options { get; }

    public DatabaseFixture()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        
        Options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ApplicationDbContext(Options);
        context.Database.EnsureCreated();
    }

    public ApplicationDbContext CreateContext() => new ApplicationDbContext(Options);

    public void Dispose() => _connection?.Dispose();
}