using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NetMinimal.Infrastructure.Data;
using NetMinimal.Infrastructure.Data.Repositories;

namespace NetMinimal.Tests.Support;

internal sealed class SqliteCatalogFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public CatalogDbContext Db { get; }
    public ProductRepository Products { get; }
    public CategoryRepository Categories { get; }
    public FakeClock Clock { get; } = new(new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero));

    public SqliteCatalogFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite(_connection)
            .UseLoggerFactory(NullLoggerFactory.Instance)
            .Options;

        Db = new CatalogDbContext(options);
        Db.Database.EnsureCreated();
        Products = new ProductRepository(Db);
        Categories = new CategoryRepository(Db);
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
