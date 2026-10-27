using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NetMinimal.Application.Contracts;
using NetMinimal.Application.Services;
using NetMinimal.Domain.Entities;
using NetMinimal.Domain.Exceptions;
using NetMinimal.Tests.Support;

namespace NetMinimal.Tests.Application;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task Create_assigns_slug_and_can_be_loaded()
    {
        await using var fx = new SqliteCatalogFixture();
        var service = CreateService(fx);

        var created = await service.CreateAsync(new CreateCategoryRequest("Hardware", "Parts"), CancellationToken.None);

        created.Slug.Should().Be("hardware");
        var loaded = await service.GetAsync(created.Id, CancellationToken.None);
        loaded.Name.Should().Be("Hardware");
    }

    [Fact]
    public async Task Create_rejects_duplicate_slug()
    {
        await using var fx = new SqliteCatalogFixture();
        var service = CreateService(fx);
        await service.CreateAsync(new CreateCategoryRequest("Hardware", null), CancellationToken.None);

        var act = () => service.CreateAsync(new CreateCategoryRequest("Hardware", "again"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*hardware*");
    }

    [Fact]
    public async Task Update_can_deactivate()
    {
        await using var fx = new SqliteCatalogFixture();
        var service = CreateService(fx);
        var created = await service.CreateAsync(new CreateCategoryRequest("Hardware", null), CancellationToken.None);

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdateCategoryRequest("Office Hardware", "desks", false),
            CancellationToken.None);

        updated.IsActive.Should().BeFalse();
        updated.Slug.Should().Be("office-hardware");

        var activeOnly = await service.ListAsync(includeInactive: false, CancellationToken.None);
        activeOnly.Should().BeEmpty();

        var all = await service.ListAsync(includeInactive: true, CancellationToken.None);
        all.Should().ContainSingle(c => c.Id == created.Id);
    }

    [Fact]
    public async Task Delete_succeeds_when_empty()
    {
        await using var fx = new SqliteCatalogFixture();
        var service = CreateService(fx);
        var created = await service.CreateAsync(new CreateCategoryRequest("Hardware", null), CancellationToken.None);

        await service.DeleteAsync(created.Id, CancellationToken.None);

        var act = () => service.GetAsync(created.Id, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_rejects_category_with_products()
    {
        await using var fx = new SqliteCatalogFixture();
        var categories = CreateService(fx);
        var created = await categories.CreateAsync(new CreateCategoryRequest("Hardware", null), CancellationToken.None);

        var products = new ProductService(fx.Products, fx.Categories, fx.Db, fx.Clock, NullLogger<ProductService>.Instance);
        await products.CreateAsync(
            new CreateProductRequest("KB-1", "Keyboard", null, 1m, 1, created.Id),
            CancellationToken.None);

        var act = () => categories.DeleteAsync(created.Id, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>().WithMessage("*still has products*");
    }

    [Fact]
    public async Task Get_unknown_id_throws_not_found()
    {
        await using var fx = new SqliteCatalogFixture();
        var service = CreateService(fx);

        var act = () => service.GetAsync(Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static CategoryService CreateService(SqliteCatalogFixture fx) =>
        new(fx.Categories, fx.Db, fx.Clock, NullLogger<CategoryService>.Instance);
}
