using FluentAssertions;
using NetMinimal.Domain.Exceptions;
using NetMinimal.Domain.StaticServices;

namespace NetMinimal.Tests.Domain;

public sealed class SlugGeneratorTests
{
    [Theory]
    [InlineData("  Mechanical Keyboard  ", "mechanical-keyboard")]
    [InlineData("C# / .NET", "c-net")]
    [InlineData("Café Crème", "cafe-creme")]
    [InlineData("Hello---World", "hello-world")]
    public void From_normalizes_name(string input, string expected)
    {
        SlugGenerator.From(input).Should().Be(expected);
    }

    [Fact]
    public void From_uses_item_when_no_alphanumeric_characters_remain()
    {
        SlugGenerator.From("@@@").Should().Be("item");
    }

    [Fact]
    public void From_rejects_blank()
    {
        var act = () => SlugGenerator.From("  ");
        act.Should().Throw<BusinessRuleException>();
    }
}
