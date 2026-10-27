using FluentAssertions;
using NetMinimal.Domain.Entities;
using NetMinimal.Tests.Support;

namespace NetMinimal.Tests.Domain;

public sealed class EntityTests
{
    [Fact]
    public void Entities_with_the_same_id_are_equal()
    {
        var left = Category.Create("A", null, CatalogData.Now);
        var right = Category.Create("B", null, CatalogData.Now);

        object.Equals(left, left).Should().BeTrue();
        object.Equals(left, right).Should().BeFalse();
        object.Equals(left, null).Should().BeFalse();
    }
}
