using Microsoft.Extensions.DependencyInjection;
using NetMinimal.Application.Abstractions;

namespace NetMinimal.Infrastructure.StaticServices;

/// <summary>
/// Process-wide service locator initialized once in <c>Program.cs</c>.
/// Prefer constructor injection. Use this only from static helpers that cannot take DI.
/// </summary>
public static class AppServices
{
    private static IServiceProvider? _provider;

    public static bool IsInitialized => _provider is not null;

    public static void Initialize(IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }

    internal static void ResetForTests() => _provider = null;

    public static IClock Clock => GetRequired<IClock>();

    public static T GetRequired<T>() where T : notnull
    {
        if (_provider is null)
        {
            throw new InvalidOperationException(
                "AppServices has not been initialized. Call AppServices.Initialize(app.Services) at startup.");
        }

        return _provider.GetRequiredService<T>();
    }

    public static IServiceScope CreateScope()
    {
        if (_provider is null)
        {
            throw new InvalidOperationException("AppServices has not been initialized.");
        }

        return _provider.CreateScope();
    }
}
