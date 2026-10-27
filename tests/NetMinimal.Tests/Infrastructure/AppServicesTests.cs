using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetMinimal.Application.Abstractions;
using NetMinimal.Infrastructure.Services;
using NetMinimal.Infrastructure.StaticServices;

namespace NetMinimal.Tests.Infrastructure;

public sealed class AppServicesTests : IDisposable
{
    public AppServicesTests()
    {
        AppServices.ResetForTests();
    }

    public void Dispose() => AppServices.ResetForTests();

    [Fact]
    public void GetRequired_throws_before_initialize()
    {
        var act = () => AppServices.GetRequired<IClock>();
        act.Should().Throw<InvalidOperationException>().WithMessage("*has not been initialized*");
        AppServices.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void Initialize_rejects_null_provider()
    {
        var act = () => AppServices.Initialize(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Clock_resolves_from_root_provider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IClock, SystemClock>();
        using var provider = services.BuildServiceProvider();

        AppServices.Initialize(provider);

        AppServices.IsInitialized.Should().BeTrue();
        AppServices.Clock.Should().BeOfType<SystemClock>();
        AppServices.GetRequired<IClock>().Should().BeSameAs(AppServices.Clock);
    }
}
