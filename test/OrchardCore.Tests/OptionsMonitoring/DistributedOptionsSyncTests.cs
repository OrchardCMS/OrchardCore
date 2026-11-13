using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Distributed;
using OrchardCore.Environment.Shell.Events;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Tests.OptionsMonitoring;

public class DistributedOptionsSyncTests
{
    private const string TenantName = "Tenant1";

    [Fact]
    public async Task OptionsUpdated_OnAnotherInstance_RefreshesOptionsAfterSynchronizationLatency()
    {
        var distributedCache = new SharedDistributedCache();

        var origin = new Instance(distributedCache, TimeSpan.FromSeconds(3));
        var other = new Instance(distributedCache, TimeSpan.FromSeconds(3));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await other.HostedService.StartAsync(cts.Token);

        try
        {
            var originVersion = origin.Monitor.CurrentValue.Version;
            var otherVersion = other.Monitor.CurrentValue.Version;

            await origin.ShellOptionsEvents.InvokeUpdatedAsync(TenantName);

            // The document identifiers cached by the other instance may not be expired yet.
            await Task.Delay(TimeSpan.FromSeconds(2), cts.Token);

            Assert.Equal(otherVersion, other.Monitor.CurrentValue.Version);

            while (other.Monitor.CurrentValue.Version == otherVersion)
            {
                await Task.Delay(100, cts.Token);
            }

            Assert.False(other.TenantContext.Released);

            // The instance that updated the options is not refreshed again.
            Assert.Equal(originVersion, origin.Monitor.CurrentValue.Version);
        }
        finally
        {
            await other.HostedService.StopAsync(CancellationToken.None);
        }
    }

    private sealed class Instance
    {
        public Instance(IDistributedCache distributedCache, TimeSpan synchronizationLatency)
        {
            var defaultContext = new ShellContext
            {
                Settings = new ShellSettings().AsDefaultShell().AsRunning(),
                Blueprint = new ShellBlueprint
                {
                    Descriptor = new ShellDescriptor
                    {
                        Features = [new ShellFeature("OrchardCore.Tenants.Distributed")],
                    },
                },
                ServiceProvider = new ServiceCollection().BuildServiceProvider(),
            };

            var version = 0;
            var tenantServices = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ISignal, Signal>()
                .AddOptions()
                .ConfigureAll<TestOptions>(options => options.Version = Interlocked.Increment(ref version))
                .AddSignalOptionsChangeTokenSource<TestOptions>();

            var tenantConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Documents:SynchronizationLatency"] = synchronizationLatency.ToString(),
                })
                .Build();

            var tenantSettings = new ShellSettings(new ShellConfiguration(), new ShellConfiguration(tenantConfiguration))
            {
                Name = TenantName,
            }.AsRunning();

            TenantContext = new ShellContext
            {
                Settings = tenantSettings,
                ServiceProvider = tenantServices.BuildServiceProvider(),
            };

            var distributedServices = new ServiceCollection()
                .AddSingleton<DistributedShellMarkerService>()
                .AddSingleton(distributedCache)
                .BuildServiceProvider();

            var shellContextFactory = new Mock<IShellContextFactory>();
            shellContextFactory
                .Setup(factory => factory.CreateDescribedContextAsync(It.IsAny<ShellSettings>(), It.IsAny<ShellDescriptor>()))
                .ReturnsAsync(() => new ShellContext
                {
                    Settings = defaultContext.Settings,
                    ServiceProvider = distributedServices,
                });

            ShellOptionsEvents = new ShellOptionsEvents();

            HostedService = new DistributedShellHostedService(
                new FakeShellHost(defaultContext, TenantContext),
                shellContextFactory.Object,
                Mock.Of<IShellSettingsManager>(),
                Mock.Of<IShellRemovalManager>(),
                ShellOptionsEvents,
                NullLogger<DistributedShellHostedService>.Instance);

            Monitor = TenantContext.ServiceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
        }

        public ShellContext TenantContext { get; }

        public ShellOptionsEvents ShellOptionsEvents { get; }

        public DistributedShellHostedService HostedService { get; }

        public IOptionsMonitor<TestOptions> Monitor { get; }
    }

    private sealed class TestOptions
    {
        public int Version { get; set; }
    }

    /// <summary>
    /// A distributed cache shared by the instances, which is not detected as a local memory cache.
    /// </summary>
    private sealed class SharedDistributedCache : IDistributedCache
    {
        private readonly MemoryDistributedCache _cache = new(Options.Create(new MemoryDistributedCacheOptions()));

        public byte[] Get(string key) => _cache.Get(key);

        public Task<byte[]> GetAsync(string key, CancellationToken token = default) => _cache.GetAsync(key, token);

        public void Refresh(string key) => _cache.Refresh(key);

        public Task RefreshAsync(string key, CancellationToken token = default) => _cache.RefreshAsync(key, token);

        public void Remove(string key) => _cache.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default) => _cache.RemoveAsync(key, token);

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _cache.Set(key, value, options);

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
            => _cache.SetAsync(key, value, options, token);
    }

    private sealed class FakeShellHost : IShellHost
    {
        private readonly Dictionary<string, ShellContext> _contexts;

        public FakeShellHost(params ShellContext[] contexts)
        {
            _contexts = contexts.ToDictionary(context => context.Settings.Name);
        }

        public ShellsEvent LoadingAsync { get; set; }

        public ShellEvent ReleasingAsync { get; set; }

        public ShellEvent ReloadingAsync { get; set; }

        public ShellEvent RemovingAsync { get; set; }

        public bool TryGetShellContext(string name, out ShellContext shellContext) => _contexts.TryGetValue(name, out shellContext);

        public bool TryGetSettings(string name, out ShellSettings settings)
        {
            settings = _contexts.TryGetValue(name, out var context) ? context.Settings : null;
            return settings is not null;
        }

        public IEnumerable<ShellSettings> GetAllSettings() => _contexts.Values.Select(context => context.Settings);

        public IEnumerable<ShellContext> ListShellContexts() => _contexts.Values;

        public Task ReleaseShellContextAsync(ShellSettings settings, bool eventSource = true) => throw new NotSupportedException();

        public Task ReloadShellContextAsync(ShellSettings settings, bool eventSource = true) => throw new NotSupportedException();

        public Task InitializeAsync() => throw new NotSupportedException();

        public Task<ShellContext> GetOrCreateShellContextAsync(ShellSettings settings) => throw new NotSupportedException();

        public Task<ShellScope> GetScopeAsync(ShellSettings settings) => throw new NotSupportedException();

        public Task UpdateShellSettingsAsync(ShellSettings settings) => throw new NotSupportedException();

        public Task RemoveShellSettingsAsync(ShellSettings settings) => throw new NotSupportedException();

        public Task RemoveShellContextAsync(ShellSettings settings, bool eventSource = true) => throw new NotSupportedException();

        public Task ChangedAsync(ShellDescriptor descriptor, ShellSettings settings) => throw new NotSupportedException();
    }
}
