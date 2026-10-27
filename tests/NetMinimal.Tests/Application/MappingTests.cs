using FluentAssertions;
using NetMinimal.Application.Mapping;
using NetMinimal.Domain.Entities;
using NetMinimal.Tests.Support;

namespace NetMinimal.Tests.Application;

public sealed class MappingTests
{
    [Fact]
    public void Category_maps_all_fields()
    {
        var category = Category.Create("Hardware", "Parts", CatalogData.Now);
        var dto = category.ToResponse();

        dto.Id.Should().Be(category.Id);
        dto.Name.Should().Be("Hardware");
        dto.Slug.Should().Be("hardware");
        dto.Description.Should().Be("Parts");
        dto.IsActive.Should().BeTrue();
        dto.CreatedAtUtc.Should().Be(CatalogData.Now);
    }

    [Fact]
    public void Product_maps_without_category_name_when_navigation_missing()
    {
        var product = Product.Create("KB-1", "Keyboard", "d", 9.99m, 2, CatalogData.CategoryId, CatalogData.Now);
        var dto = product.ToResponse();

        dto.Sku.Should().Be("KB-1");
        dto.Price.Should().Be(9.99m);
        dto.Stock.Should().Be(2);
        dto.CategoryId.Should().Be(CatalogData.CategoryId);
        dto.CategoryName.Should().BeNull();
    }
}
