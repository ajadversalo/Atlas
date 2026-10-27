using FluentAssertions;
using NetMinimal.Application.Common;

namespace NetMinimal.Tests.Application;

public sealed class PagedResultTests
{
    [Fact]
    public void Computes_page_metadata()
    {
        var page = new PagedResult<int>([1, 2], TotalCount: 5, Page: 2, PageSize: 2);

        page.TotalPages.Should().Be(3);
        page.HasNext.Should().BeTrue();
        page.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void First_page_has_no_previous()
    {
        var page = new PagedResult<int>([1], TotalCount: 1, Page: 1, PageSize: 25);

        page.TotalPages.Should().Be(1);
        page.HasNext.Should().BeFalse();
        page.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void Zero_page_size_avoids_divide_by_zero()
    {
        var page = new PagedResult<int>([], TotalCount: 10, Page: 1, PageSize: 0);
        page.TotalPages.Should().Be(0);
    }
}
