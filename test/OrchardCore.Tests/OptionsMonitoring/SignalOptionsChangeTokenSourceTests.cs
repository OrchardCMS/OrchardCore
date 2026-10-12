using Microsoft.Extensions.Options;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Options;

namespace OrchardCore.Tests.OptionsMonitoring;

public class SignalOptionsChangeTokenSourceTests
{
    [Fact]
    public async Task SignalTokenAsync_DefaultName_RefreshesDefaultOptions()
    {
        using var provider = CreateServiceProvider(services => services.AddSignalOptionsChangeTokenSource<TestOptions>());

        var monitor = provider.GetRequiredService<IOptionsMonitor<TestOptions>>();
        var signal = provider.GetRequiredService<ISignal>();

        var version = monitor.CurrentValue.Version;

        await signal.SignalTokenAsync(OptionsUpdateSignal.GetKey(typeof(TestOptions), string.Empty));

        Assert.NotEqual(version, monitor.CurrentValue.Version);
    }

    [Fact]
    public async Task SignalTokenAsync_NamedSource_RefreshesOnlyThatName()
    {
        using var provider = CreateServiceProvider(services => services
            .AddSignalOptionsChangeTokenSource<TestOptions>("First")
            .AddSignalOptionsChangeTokenSource<TestOptions>("Second"));

        var monitor = provider.GetRequiredService<IOptionsMonitor<TestOptions>>();
        var signal = provider.GetRequiredService<ISignal>();

        var first = monitor.Get("First").Version;
        var second = monitor.Get("Second").Version;

        await signal.SignalTokenAsync(OptionsUpdateSignal.GetKey(typeof(TestOptions), "First"));

        Assert.NotEqual(first, monitor.Get("First").Version);
        Assert.Equal(second, monitor.Get("Second").Version);

        second = monitor.Get("Second").Version;

        await signal.SignalTokenAsync(OptionsUpdateSignal.GetKey(typeof(TestOptions), "Second"));

        Assert.NotEqual(second, monitor.Get("Second").Version);
    }

    [Fact]
    public void AddSignalOptionsChangeTokenSource_SameNameTwice_RegistersOneSource()
    {
        using var provider = CreateServiceProvider(services => services
            .AddSignalOptionsChangeTokenSource<TestOptions>()
            .AddSignalOptionsChangeTokenSource<TestOptions>()
            .AddSignalOptionsChangeTokenSource<TestOptions>("First")
            .AddSignalOptionsChangeTokenSource<TestOptions>("First")
            .AddSignalOptionsChangeTokenSource<TestOptions>(string.Empty));

        var sources = provider.GetServices<IOptionsChangeTokenSource<TestOptions>>()
            .Select(source => source.Name)
            .Order()
            .ToArray();

        Assert.Equal([string.Empty, "First"], sources);
    }

    [Fact]
    public async Task SignalTokenAsync_AllKey_RefreshesEveryObservedOptions()
    {
        using var provider = CreateServiceProvider(services => services
            .AddSignalOptionsChangeTokenSource<TestOptions>()
            .AddSignalOptionsChangeTokenSource<TestOptions>("First")
            .AddSignalOptionsChangeTokenSource<OtherOptions>());

        var monitor = provider.GetRequiredService<IOptionsMonitor<TestOptions>>();
        var otherMonitor = provider.GetRequiredService<IOptionsMonitor<OtherOptions>>();
        var signal = provider.GetRequiredService<ISignal>();

        for (var i = 0; i < 2; i++)
        {
            var defaultVersion = monitor.CurrentValue.Version;
            var namedVersion = monitor.Get("First").Version;
            var otherVersion = otherMonitor.CurrentValue.Version;

            await signal.SignalTokenAsync(OptionsUpdateSignal.AllKey);

            Assert.NotEqual(defaultVersion, monitor.CurrentValue.Version);
            Assert.NotEqual(namedVersion, monitor.Get("First").Version);
            Assert.NotEqual(otherVersion, otherMonitor.CurrentValue.Version);
        }
    }

    [Fact]
    public async Task SignalTokenAsync_OtherOptionsKey_DoesNotRefresh()
    {
        using var provider = CreateServiceProvider(services => services
            .AddSignalOptionsChangeTokenSource<TestOptions>()
            .AddSignalOptionsChangeTokenSource<OtherOptions>());

        var monitor = provider.GetRequiredService<IOptionsMonitor<TestOptions>>();
        var signal = provider.GetRequiredService<ISignal>();

        var version = monitor.CurrentValue.Version;

        await signal.SignalTokenAsync(OptionsUpdateSignal.GetKey(typeof(OtherOptions), string.Empty));

        Assert.Equal(version, monitor.CurrentValue.Version);
    }

    private static ServiceProvider CreateServiceProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        var version = 0;

        services.AddSingleton<ISignal, Signal>();
        services.AddOptions();
        services.ConfigureAll<TestOptions>(options => options.Version = Interlocked.Increment(ref version));
        services.ConfigureAll<OtherOptions>(options => options.Version = Interlocked.Increment(ref version));

        configure(services);

        return services.BuildServiceProvider();
    }

    private sealed class TestOptions
    {
        public int Version { get; set; }
    }

    private sealed class OtherOptions
    {
        public int Version { get; set; }
    }
}
