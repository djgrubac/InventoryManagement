using Inventory_Management.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace Inventory_Management.Application.IntegrationTests.Testing;

/// <summary>
/// Gives every test its own in-memory SQLite database. The connection stays open for the whole test
/// (closing it drops the database), and each <see cref="CreateContext"/> call returns a fresh context on it,
/// so arrange / act / assert can use separate change trackers and a missing Include can't hide behind
/// entities that are already tracked.
/// </summary>
public abstract class IntegrationTestBase
{
    private SqliteConnection _connection = null!;
    private DbContextOptions<ApplicationDbContext> _options = null!;
    private readonly List<ApplicationDbContext> _contexts = new();

    [SetUp]
    public void SetUpDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new SqliteApplicationDbContext(_options);
        context.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDownDatabase()
    {
        foreach (var context in _contexts)
            context.Dispose();
        _contexts.Clear();
        _connection.Dispose();
    }

    protected ApplicationDbContext CreateContext()
    {
        var context = new SqliteApplicationDbContext(_options);
        _contexts.Add(context);
        return context;
    }

    /// <summary>
    /// Saves entity graphs. Attach (not Add) means rows saved by an earlier seed (key already set) are left
    /// Unchanged while new ones (key 0) are inserted, so graphs can reference previously seeded entities.
    /// </summary>
    protected async Task SeedAsync(params object[] entities)
    {
        await using var context = new SqliteApplicationDbContext(_options);
        context.AttachRange(entities);
        await context.SaveChangesAsync();
    }
}
