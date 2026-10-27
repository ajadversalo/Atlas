using FluentAssertions;
using NetMinimal.Domain.StaticServices;

namespace NetMinimal.Tests.Domain;

public sealed class ClockTests
{
    [Fact]
    public void Freeze_returns_fixed_timestamp_until_disposed()
    {
        var frozen = new DateTimeOffset(2026, 6, 1, 8, 30, 0, TimeSpan.Zero);

        using (Clock.Freeze(frozen))
        {
            Clock.UtcNow.Should().Be(frozen);
        }

        Clock.UtcNow.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
