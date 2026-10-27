using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NetMinimal.Infrastructure.Data;
using NetMinimal.Tests.Support;

namespace NetMinimal.Tests.Infrastructure;

public sealed class CatalogSeederTests
{
    [Fact]
    public async Task Seed_inserts_catalog_once()
    {
        await using var fx = new SqliteCatalogFixture();
        var seeder = new CatalogSeeder(fx.Db, fx.Clock, NullLogger<CatalogSeeder>.Instance);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        (await fx.Db.Categories.CountAsync()).Should().Be(2);
        (await fx.Db.Products.CountAsync()).Should().Be(3);
        (await fx.Db.Products.AnyAsync(p => p.Sku == "KB-001")).Should().BeTrue();
    }
}
