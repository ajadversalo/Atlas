using FluentAssertions;
using NetMinimal.Infrastructure.Services;

namespace NetMinimal.Tests.Infrastructure;

public sealed class SystemClockTests
{
    [Fact]
    public void UtcNow_is_close_to_system_time()
    {
        var clock = new SystemClock();
        clock.UtcNow.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        clock.UtcNow.Offset.Should().Be(TimeSpan.Zero);
    }
}
