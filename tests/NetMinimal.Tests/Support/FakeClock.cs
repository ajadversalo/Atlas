using NetMinimal.Application.Abstractions;

namespace NetMinimal.Tests.Support;

internal sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}
