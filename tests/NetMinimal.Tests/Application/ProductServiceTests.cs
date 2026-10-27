using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NetMinimal.Application.Abstractions;
using NetMinimal.Application.Contracts;
using NetMinimal.Application.Services;
using NetMinimal.Domain.Entities;
using NetMinimal.Domain.Exceptions;
using NetMinimal.Tests.Support;

namespace NetMinimal.Tests.Application;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task Create_persists_product_under_existing_category()
    {
        await using var fx = new SqliteCatalogFixture();
        var category = await SeedCategory(fx);
        var service = CreateService(fx);

        var created = await service.CreateAsync(
            new CreateProductRequest("kb-001", "Keyboard", "Tactile", 99.99m, 10, category.Id),
            CancellationToken.None);

        created.Sku.Should().Be("KB-001");
        created.Price.Should().Be(99.99m);
        created.CategoryId.Should().Be(category.Id);

        var loaded = await service.GetAsync(created.Id, CancellationToken.None);
        loaded.Name.Should().Be("Keyboard");
    }

    [Fact]
    public async Task Create_rejects_duplicate_sku()
    {
        await using var fx = new SqliteCatalogFixture();
        var category = await SeedCategory(fx);
        var service = CreateService(fx);
        var request = new CreateProductRequest("KB-001", "Keyboard", null, 10m, 1, category.Id);
        await service.CreateAsync(request, CancellationToken.None);

        var act = () => service.CreateAsync(request with { Name = "Other" }, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_rejects_unknown_category()
    {
        await using var fx = new SqliteCatalogFixture();
        var service = CreateService(fx);

        var act = () => service.CreateAsync(
            new CreateProductRequest("X-1", "Thing", null, 1m, 0, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_rejects_inactive_category()
    {
        await using var fx = new SqliteCatalogFixture();
        var category = await SeedCategory(fx);
        category.Update(category.Name, category.Description, isActive: false, fx.Clock.UtcNow);
        await fx.Db.SaveChangesAsync();
        var service = CreateService(fx);

        var act = () => service.CreateAsync(
            new CreateProductRequest("X-1", "Thing", null, 1m, 0, category.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*inactive category*");
    }

    [Fact]
    public async Task Update_changes_name_and_price()
    {
        await using var fx = new SqliteCatalogFixture();
        var category = await SeedCategory(fx);
        var service = CreateService(fx);
        var created = await service.CreateAsync(
            new CreateProductRequest("KB-001", "Keyboard", null, 10m, 1, category.Id),
            CancellationToken.None);

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdateProductRequest("Keeb", "hotswap", 12.5m, true, category.Id),
            CancellationToken.None);

        updated.Name.Should().Be("Keeb");
        updated.Description.Should().Be("hotswap");
        updated.Price.Should().Be(12.50m);
        updated.Sku.Should().Be("KB-001");
    }

    [Fact]
    public async Task AdjustStock_applies_delta()
    {
        await using var fx = new SqliteCatalogFixture();
        var category = await SeedCategory(fx);
        var service = CreateService(fx);
        var created = await service.CreateAsync(
            new CreateProductRequest("MS-1", "Mouse", null, 20m, 2, category.Id),
            CancellationToken.None);

        var updated = await service.AdjustStockAsync(created.Id, new AdjustStockRequest(3), CancellationToken.None);

        updated.Stock.Should().Be(5);
    }

    [Fact]
    public async Task AdjustStock_rejects_negative_result()
    {
        await using var fx = new SqliteCatalogFixture();
        var category = await SeedCategory(fx);
        var service = CreateService(fx);
        var created = await service.CreateAsync(
            new CreateProductRequest("MS-1", "Mouse", null, 20m, 2, category.Id),
            CancellationToken.None);

        var act = () => service.AdjustStockAsync(created.Id, new AdjustStockRequest(-5), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Delete_removes_product()
    {
        await using var fx = new SqliteCatalogFixture();
        var category = await SeedCategory(fx);
        var service = CreateService(fx);
        var created = await service.CreateAsync(
            new CreateProductRequest("MS-1", "Mouse", null, 20m, 2, category.Id),
            CancellationToken.None);

        await service.DeleteAsync(created.Id, CancellationToken.None);

        var act = () => service.GetAsync(created.Id, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Query_filters_by_search_and_pages()
    {
        await using var fx = new SqliteCatalogFixture();
        var category = await SeedCategory(fx);
        var service = CreateService(fx);
        await service.CreateAsync(new CreateProductRequest("KB-001", "Keyboard", null, 1m, 1, category.Id), CancellationToken.None);
        await service.CreateAsync(new CreateProductRequest("MS-001", "Mouse", null, 1m, 1, category.Id), CancellationToken.None);
        await service.CreateAsync(new CreateProductRequest("CAM-001", "Camera", null, 1m, 1, category.Id), CancellationToken.None);

        var page = await service.QueryAsync(
            new ProductQuery("KB", category.Id, IncludeInactive: false, Page: 1, PageSize: 10),
            CancellationToken.None);

        page.TotalCount.Should().Be(1);
        page.Items.Should().ContainSingle(p => p.Sku == "KB-001");
        page.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task Query_rejects_invalid_page()
    {
        await using var fx = new SqliteCatalogFixture();
        var service = CreateService(fx);

        var act = () => service.QueryAsync(
            new ProductQuery(null, null, false, Page: 0, PageSize: 25),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Get_unknown_id_throws_not_found()
    {
        await using var fx = new SqliteCatalogFixture();
        var service = CreateService(fx);

        var act = () => service.GetAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static async Task<Category> SeedCategory(SqliteCatalogFixture fx)
    {
        var category = Category.Create("Hardware", null, fx.Clock.UtcNow);
        await fx.Categories.AddAsync(category);
        await fx.Db.SaveChangesAsync();
        return category;
    }

    private static ProductService CreateService(SqliteCatalogFixture fx) =>
        new(fx.Products, fx.Categories, fx.Db, fx.Clock, NullLogger<ProductService>.Instance);
}
