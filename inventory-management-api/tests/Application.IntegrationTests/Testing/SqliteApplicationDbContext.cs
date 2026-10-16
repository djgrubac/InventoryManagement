using Inventory_Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Inventory_Management.Application.IntegrationTests.Testing;

/// <summary>
/// The production context with the one tweak SQLite needs: it can't ORDER BY a DateTimeOffset column,
/// so those are stored as their binary (sortable) long form. Everything else, including the citext
/// string convention (TEXT affinity in SQLite), comes from <see cref="ApplicationDbContext"/>.
/// Note: string comparisons/ordering are case-sensitive here, unlike citext on Postgres.
/// </summary>
public class SqliteApplicationDbContext : ApplicationDbContext
{
    public SqliteApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
    }
}
