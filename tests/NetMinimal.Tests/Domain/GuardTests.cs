using FluentAssertions;
using NetMinimal.Domain.Exceptions;
using NetMinimal.Domain.StaticServices;

namespace NetMinimal.Tests.Domain;

public sealed class GuardTests
{
    [Fact]
    public void AgainstNull_returns_instance()
    {
        var value = new object();
        Guard.AgainstNull(value).Should().BeSameAs(value);
    }

    [Fact]
    public void AgainstNull_rejects_null()
    {
        object? value = null;
        var act = () => Guard.AgainstNull(value);
        act.Should().Throw<BusinessRuleException>().WithMessage("*value is required*");
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_trims_value()
    {
        Guard.AgainstNullOrWhiteSpace("  sku  ").Should().Be("sku");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgainstNullOrWhiteSpace_rejects_blank(string? value)
    {
        var act = () => Guard.AgainstNullOrWhiteSpace(value);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_rejects_too_long()
    {
        var act = () => Guard.AgainstNullOrWhiteSpace("abcd", maxLength: 3);
        act.Should().Throw<BusinessRuleException>().WithMessage("*cannot exceed 3 characters*");
    }

    [Fact]
    public void Optional_returns_null_for_blank()
    {
        Guard.Optional("  ").Should().BeNull();
        Guard.Optional(null).Should().BeNull();
    }

    [Fact]
    public void Optional_trims_and_rejects_too_long()
    {
        Guard.Optional("  hi  ").Should().Be("hi");
        var act = () => Guard.Optional("toolong", maxLength: 3);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void AgainstEmpty_rejects_default_guid()
    {
        var act = () => Guard.AgainstEmpty(Guid.Empty);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void AgainstEmpty_returns_value()
    {
        var id = Guid.NewGuid();
        Guard.AgainstEmpty(id).Should().Be(id);
    }

    [Theory]
    [InlineData(0, 1, 10)]
    [InlineData(11, 1, 10)]
    public void AgainstOutOfRange_rejects_outside_bounds(int value, int min, int max)
    {
        var act = () => Guard.AgainstOutOfRange(value, min, max);
        act.Should().Throw<BusinessRuleException>().WithMessage("*between 1 and 10*");
    }

    [Fact]
    public void AgainstOutOfRange_returns_in_range_value()
    {
        Guard.AgainstOutOfRange(1, 1, 10).Should().Be(1);
        Guard.AgainstOutOfRange(10, 1, 10).Should().Be(10);
    }
}
