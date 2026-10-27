using FluentAssertions;
using NetMinimal.Domain.Entities;
using NetMinimal.Domain.Exceptions;
using NetMinimal.Tests.Support;

namespace NetMinimal.Tests.Domain;

public sealed class CategoryTests
{
    [Fact]
    public void Create_trims_name_and_builds_slug()
    {
        var category = Category.Create("  Hardware  ", "  Physical  ", CatalogData.Now);

        category.Name.Should().Be("Hardware");
        category.Slug.Should().Be("hardware");
        category.Description.Should().Be("Physical");
        category.IsActive.Should().BeTrue();
        category.CreatedAtUtc.Should().Be(CatalogData.Now);
        category.UpdatedAtUtc.Should().Be(CatalogData.Now);
        category.Products.Should().BeEmpty();
    }

    [Fact]
    public void Create_rejects_blank_name()
    {
        var act = () => Category.Create("  ", null, CatalogData.Now);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Update_regenerates_slug_and_can_deactivate()
    {
        var category = Category.Create("Hardware", null, CatalogData.Now);
        var later = CatalogData.Now.AddDays(1);

        category.Update("Office Hardware", "desks", false, later);

        category.Name.Should().Be("Office Hardware");
        category.Slug.Should().Be("office-hardware");
        category.Description.Should().Be("desks");
        category.IsActive.Should().BeFalse();
        category.CreatedAtUtc.Should().Be(CatalogData.Now);
        category.UpdatedAtUtc.Should().Be(later);
    }
}
