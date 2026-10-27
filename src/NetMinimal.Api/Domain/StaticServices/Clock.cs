namespace NetMinimal.Domain.StaticServices;

/// <summary>
/// Testable static clock. Production code should prefer <c>IClock</c> via DI.
/// Tests and entities that cannot take constructor injection can use this.
/// </summary>
public static class Clock
{
    private static readonly AsyncLocal<Func<DateTimeOffset>?> Override = new();

    public static DateTimeOffset UtcNow => (Override.Value ?? (() => DateTimeOffset.UtcNow))();

    public static IDisposable Freeze(DateTimeOffset utcNow)
    {
        Override.Value = () => utcNow;
        return new Reset();
    }

    private sealed class Reset : IDisposable
    {
        public void Dispose() => Override.Value = null;
    }
}
