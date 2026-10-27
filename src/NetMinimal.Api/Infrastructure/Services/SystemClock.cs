using NetMinimal.Application.Abstractions;

namespace NetMinimal.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
