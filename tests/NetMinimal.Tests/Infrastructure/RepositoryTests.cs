using FluentAssertions;
using NetMinimal.Application.Abstractions;
using NetMinimal.Domain.Entities;
using NetMinimal.Tests.Support;

namespace NetMinimal.Tests.Infrastructure;

public sealed class RepositoryTests
{
    [Fact]
    public async Task Product_query_filters_search_category_and_inactive()
    {
        await using var fx = new SqliteCatalogFixture();
        var hardware = Category.Create("Hardware", null, fx.Clock.UtcNow);
        var software = Category.Create("Software", null, fx.Clock.UtcNow);
        await fx.Categories.AddAsync(hardware);
        await fx.Categories.AddAsync(software);

        var keyboard = Product.Create("KB-001", "Keyboard", null, 1m, 1, hardware.Id, fx.Clock.UtcNow);
        var mouse = Product.Create("MS-001", "Mouse", null, 1m, 1, hardware.Id, fx.Clock.UtcNow);
        var license = Product.Create("SW-001", "License", null, 1m, 1, software.Id, fx.Clock.UtcNow);
        mouse.Update(mouse.Name, mouse.Description, mouse.Price, isActive: false, mouse.CategoryId, fx.Clock.UtcNow);

        await fx.Products.AddAsync(keyboard);
        await fx.Products.AddAsync(mouse);
        await fx.Products.AddAsync(license);
        await fx.Db.SaveChangesAsync();

        var activeHardware = await fx.Products.QueryAsync(
            new ProductQuery(null, hardware.Id, IncludeInactive: false, Page: 1, PageSize: 10),
            CancellationToken.None);
        activeHardware.TotalCount.Should().Be(1);
        activeHardware.Items.Should().ContainSingle(p => p.Sku == "KB-001");

        var includingInactive = await fx.Products.QueryAsync(
            new ProductQuery(null, hardware.Id, IncludeInactive: true, Page: 1, PageSize: 10),
            CancellationToken.None);
        includingInactive.TotalCount.Should().Be(2);

        var search = await fx.Products.QueryAsync(
            new ProductQuery("KB", null, IncludeInactive: true, Page: 1, PageSize: 10),
            CancellationToken.None);
        search.Items.Should().ContainSingle(p => p.Sku == "KB-001");
    }

    [Fact]
    public async Task GetBySku_normalizes_lookup()
    {
        await using var fx = new SqliteCatalogFixture();
        var hardware = Category.Create("Hardware", null, fx.Clock.UtcNow);
        await fx.Categories.AddAsync(hardware);
        await fx.Products.AddAsync(Product.Create("kb-001", "Keyboard", null, 1m, 1, hardware.Id, fx.Clock.UtcNow));
        await fx.Db.SaveChangesAsync();

        var found = await fx.Products.GetBySkuAsync("  kb-001  ");
        found.Should().NotBeNull();
        found!.Sku.Should().Be("KB-001");
    }

    [Fact]
    public async Task Category_list_and_has_products()
    {
        await using var fx = new SqliteCatalogFixture();
        var hardware = Category.Create("Hardware", null, fx.Clock.UtcNow);
        var software = Category.Create("Software", null, fx.Clock.UtcNow);
        software.Update("Software", null, isActive: false, fx.Clock.UtcNow);
        await fx.Categories.AddAsync(hardware);
        await fx.Categories.AddAsync(software);
        await fx.Products.AddAsync(Product.Create("KB-1", "Keyboard", null, 1m, 1, hardware.Id, fx.Clock.UtcNow));
        await fx.Db.SaveChangesAsync();

        var active = await fx.Categories.ListAsync(includeInactive: false);
        active.Should().ContainSingle(c => c.Name == "Hardware");

        (await fx.Categories.HasProductsAsync(hardware.Id)).Should().BeTrue();
        (await fx.Categories.HasProductsAsync(software.Id)).Should().BeFalse();
        (await fx.Categories.GetBySlugAsync("hardware")).Should().NotBeNull();
    }
}
