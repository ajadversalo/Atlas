using FluentAssertions;
using NetMinimal.Domain.StaticServices;

namespace NetMinimal.Tests.Domain;

public sealed class TextTests
{
    [Fact]
    public void NormalizeSku_trims_and_uppercases()
    {
        Text.NormalizeSku("  kb-001  ").Should().Be("KB-001");
    }
}
