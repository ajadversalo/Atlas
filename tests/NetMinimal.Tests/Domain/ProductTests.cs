using FluentAssertions;
using NetMinimal.Domain.Entities;
using NetMinimal.Domain.Exceptions;
using NetMinimal.Tests.Support;

namespace NetMinimal.Tests.Domain;

public sealed class ProductTests
{
    [Fact]
    public void Create_normalizes_sku_rounds_price_and_stamps_timestamps()
    {
        var product = Product.Create(" kb-001 ", " Keyboard ", "  desc  ", 10.125m, 3, CatalogData.CategoryId, CatalogData.Now);

        product.Sku.Should().Be("KB-001");
        product.Name.Should().Be("Keyboard");
        product.Description.Should().Be("desc");
        product.Price.Should().Be(10.13m);
        product.Stock.Should().Be(3);
        product.IsActive.Should().BeTrue();
        product.CategoryId.Should().Be(CatalogData.CategoryId);
        product.CreatedAtUtc.Should().Be(CatalogData.Now);
        product.UpdatedAtUtc.Should().Be(CatalogData.Now);
        product.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_rejects_empty_category()
    {
        var act = () => Product.Create("SKU", "Name", null, 1m, 0, Guid.Empty, CatalogData.Now);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Create_rejects_negative_price()
    {
        var act = () => Product.Create("SKU", "Name", null, -0.01m, 0, CatalogData.CategoryId, CatalogData.Now);
        act.Should().Throw<BusinessRuleException>().WithMessage("*Price cannot be negative*");
    }

    [Fact]
    public void Create_rejects_negative_stock()
    {
        var act = () => Product.Create("SKU", "Name", null, 1m, -1, CatalogData.CategoryId, CatalogData.Now);
        act.Should().Throw<BusinessRuleException>().WithMessage("*Stock cannot be negative*");
    }

    [Fact]
    public void Update_changes_fields_and_keeps_sku()
    {
        var product = Product.Create("KB-001", "Keyboard", null, 10m, 1, CatalogData.CategoryId, CatalogData.Now);
        var later = CatalogData.Now.AddHours(1);
        var otherCategory = Guid.NewGuid();

        product.Update("New", "d", 12.5m, false, otherCategory, later);

        product.Sku.Should().Be("KB-001");
        product.Name.Should().Be("New");
        product.Description.Should().Be("d");
        product.Price.Should().Be(12.50m);
        product.IsActive.Should().BeFalse();
        product.CategoryId.Should().Be(otherCategory);
        product.CreatedAtUtc.Should().Be(CatalogData.Now);
        product.UpdatedAtUtc.Should().Be(later);
    }

    [Fact]
    public void AdjustStock_increments_and_decrements()
    {
        var product = Product.Create("MS-1", "Mouse", null, 20m, 2, CatalogData.CategoryId, CatalogData.Now);
        product.AdjustStock(3, CatalogData.Now.AddMinutes(1));
        product.Stock.Should().Be(5);

        product.AdjustStock(-5, CatalogData.Now.AddMinutes(2));
        product.Stock.Should().Be(0);
    }

    [Fact]
    public void AdjustStock_rejects_result_below_zero()
    {
        var product = Product.Create("MS-1", "Mouse", null, 20m, 2, CatalogData.CategoryId, CatalogData.Now);
        var act = () => product.AdjustStock(-3, CatalogData.Now);
        act.Should().Throw<BusinessRuleException>().WithMessage("*MS-1*");
    }
}
