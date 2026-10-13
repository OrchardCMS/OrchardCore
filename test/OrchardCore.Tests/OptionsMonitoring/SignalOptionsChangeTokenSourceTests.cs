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
    public async Task SignalAllAsync_OptionsBuiltFromOtherOptions_RebuildsFromUpdatedOptions()
    {
        var store = new SourceStore { Value = 1 };

        var services = new ServiceCollection()
            .AddSingleton<ISignal, Signal>()
            .AddSingleton(store)
            .AddOptions()
            .AddSignalOptionsChangeTokenSource<SourceOptions>()
            .AddSignalOptionsChangeTokenSource<DerivedOptions>()
            .AddSignalOptionsChangeTokenSource<TopOptions>();

        services.AddTransient<IConfigureOptions<SourceOptions>, ConfigureSourceOptions>();
        services.AddTransient<IConfigureOptions<DerivedOptions>, ConfigureDerivedOptions>();
        services.AddTransient<IConfigureOptions<TopOptions>, ConfigureTopOptions>();

        using var provider = services.BuildServiceProvider();

        var monitor = provider.GetRequiredService<IOptionsMonitor<TopOptions>>();
        Assert.Equal(1, monitor.CurrentValue.Value);

        store.Value = 2;

        await OptionsUpdateSignal.SignalAllAsync(provider.GetRequiredService<ISignal>());

        Assert.Equal(2, monitor.CurrentValue.Value);
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

    private sealed class SourceStore
    {
        public int Value { get; set; }
    }

    private sealed class SourceOptions
    {
        public int Value { get; set; }
    }

    private sealed class DerivedOptions
    {
        public int Value { get; set; }
    }

    private sealed class TopOptions
    {
        public int Value { get; set; }
    }

    private sealed class ConfigureSourceOptions : IConfigureOptions<SourceOptions>
    {
        private readonly SourceStore _store;

        public ConfigureSourceOptions(SourceStore store) => _store = store;

        public void Configure(SourceOptions options) => options.Value = _store.Value;
    }

    private sealed class ConfigureDerivedOptions : IConfigureOptions<DerivedOptions>
    {
        private readonly IOptionsMonitor<SourceOptions> _sourceOptions;

        public ConfigureDerivedOptions(IOptionsMonitor<SourceOptions> sourceOptions) => _sourceOptions = sourceOptions;

        public void Configure(DerivedOptions options) => options.Value = _sourceOptions.CurrentValue.Value;
    }

    private sealed class ConfigureTopOptions : IConfigureOptions<TopOptions>
    {
        private readonly IOptionsMonitor<DerivedOptions> _derivedOptions;

        public ConfigureTopOptions(IOptionsMonitor<DerivedOptions> derivedOptions) => _derivedOptions = derivedOptions;

        public void Configure(TopOptions options) => options.Value = _derivedOptions.CurrentValue.Value;
    }
}
