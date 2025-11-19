using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking;
using OrchardCore.Modules.Manifest;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement.Descriptors;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "")]
public class DefaultShapeTableManagerBindingsTests : IDisposable
{
    private readonly IServiceProvider _sp;

    private sealed class ModuleExtensionInfo : IExtensionInfo
    {
        public ModuleExtensionInfo(string id)
        {
            Id = id;
            Manifest = new ManifestInfo(new ModuleAttribute());
            SubPath = id; // arbitrary
        }

        public IFileInfo ExtensionFileInfo { get; set; }
        public IEnumerable<IFeatureInfo> Features { get; set; } = Array.Empty<IFeatureInfo>();
        public string Id { get; set; }
        public IManifestInfo Manifest { get; set; }
        public string SubPath { get; set; }
        public bool Exists => true;
    }

    private sealed class TestExtensionManager : IExtensionManager
    {
        private readonly IFeatureInfo[] _features;
        public TestExtensionManager(params IFeatureInfo[] features) => _features = features;

        public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId)
        {
            var feature = _features.First(x => x.Id == featureId);
            return _features.Where(x => feature.Dependencies.Contains(x.Id));
        }

        public IExtensionInfo GetExtension(string extensionId) => _features.Select(x => x.Extension).First(x => x.Id == extensionId);
        public IEnumerable<IExtensionInfo> GetExtensions() => _features.Select(x => x.Extension).Distinct();
        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo) => throw new NotImplementedException();
        public Task<IEnumerable<ExtensionEntry>> LoadExtensionsAsync(IEnumerable<IExtensionInfo> extensionInfos) => throw new NotImplementedException();
        public Task<IFeatureInfo> LoadFeatureAsync(IFeatureInfo feature) => Task.FromResult(feature);
        public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(IEnumerable<IFeatureInfo> features) => Task.FromResult(features);
        public IEnumerable<IFeatureInfo> GetFeatures() => _features;
        public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad) => GetFeatures((IEnumerable<string>)featureIdsToLoad);
        public IEnumerable<IFeatureInfo> GetFeatures(IEnumerable<string> featureIdsToLoad) => _features.Where(x => featureIdsToLoad.Contains(x.Id));
        public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId) => throw new NotImplementedException();
        public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync() => throw new NotImplementedException();
        public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(string[] featureIdsToLoad) => LoadFeaturesAsync((IEnumerable<string>)featureIdsToLoad);
        public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(IEnumerable<string> featureIdsToLoad) => throw new NotImplementedException();
    }

    private sealed class TestShellFeaturesManager : IShellFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;
        public TestShellFeaturesManager(IExtensionManager extensionManager) => _extensionManager = extensionManager;
        public Task<IEnumerable<IFeatureInfo>> GetEnabledFeaturesAsync() => Task.FromResult(_extensionManager.GetFeatures());
        public Task<IEnumerable<IFeatureInfo>> GetAlwaysEnabledFeaturesAsync() => throw new NotImplementedException();
        public Task<IEnumerable<IFeatureInfo>> GetDisabledFeaturesAsync() => throw new NotImplementedException();
        public Task<(IEnumerable<IFeatureInfo>, IEnumerable<IFeatureInfo>)> UpdateFeaturesAsync(IEnumerable<IFeatureInfo> featuresToDisable, IEnumerable<IFeatureInfo> featuresToEnable, bool force) => throw new NotImplementedException();
        public Task<IEnumerable<IExtensionInfo>> GetEnabledExtensionsAsync() => Task.FromResult(_extensionManager.GetExtensions());
        public Task<IEnumerable<IFeatureInfo>> GetAvailableFeaturesAsync() => Task.FromResult(_extensionManager.GetFeatures());
        public Task<IEnumerable<IExtensionInfo>> GetAvailableExtensionsAsync() => Task.FromResult(_extensionManager.GetExtensions());
    }

    private sealed class MultiBindingTestShapeProvider : IShapeTableProvider
    {
        public static IFeatureInfo FeatureA1;
        public static IFeatureInfo FeatureA2;
        public static IFeatureInfo FeatureB1;
        public static void Init(IFeatureInfo a1, IFeatureInfo a2, IFeatureInfo b1)
        {
            FeatureA1 = a1; FeatureA2 = a2; FeatureB1 = b1;
        }

        public ValueTask DiscoverAsync(ShapeTableBuilder builder)
        {
            // Two bindings with the same binding source from the same extension (ModuleA) via two different features.
            builder.Describe("TestShape").From(FeatureA1).BoundAs("CommonSource", null);
            builder.Describe("TestShape").From(FeatureA2).BoundAs("CommonSource", null);

            // Another module depends on A1 and overrides the shape with its own binding source.
            builder.Describe("TestShape").From(FeatureB1).BoundAs("OverrideSource", null);

            return ValueTask.CompletedTask;
        }
    }

    public DefaultShapeTableManagerBindingsTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddScoped<IShellFeaturesManager, TestShellFeaturesManager>();
        services.AddScoped<IShapeTableManager, DefaultShapeTableManager>();
        services.AddKeyedSingleton<IDictionary<string, Task<ShapeTable>>>(nameof(DefaultShapeTableManager), new ConcurrentDictionary<string, Task<ShapeTable>>());
        services.AddSingleton<ILocalLock, LocalLock>();
        services.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();
        services.AddSingleton<IHostEnvironment>(new StubHostingEnvironment());

        // Define ModuleA with two features: A1 and A2.
        var moduleA = new ModuleExtensionInfo("ModuleA");
        var A1 = new FeatureInfo("ModuleA.A1", "ModuleA.A1", 0, string.Empty, string.Empty, moduleA, Array.Empty<string>(), false, false, false);
        var A2 = new FeatureInfo("ModuleA.A2", "ModuleA.A2", 0, string.Empty, string.Empty, moduleA, Array.Empty<string>(), false, false, false);
        moduleA.Features = new[] { A1, A2 };

        // Define ModuleB with one feature B1 that depends on A1.
        var moduleB = new ModuleExtensionInfo("ModuleB");
        var B1 = new FeatureInfo("ModuleB.B1", "ModuleB.B1", 0, string.Empty, string.Empty, moduleB, new[] { A1.Id }, false, false, false);
        moduleB.Features = new[] { B1 };

        // Enabled features order is A1, then B1 (depends on A1), then A2 (same module as A1).
        var features = new IFeatureInfo[] { A1, B1, A2 };
        services.AddSingleton<IExtensionManager>(new TestExtensionManager(features));

        // Register our provider and map it to a feature (arbitrary, A1 is fine).
        services.AddShapeTableProvider<MultiBindingTestShapeProvider>();

        _sp = services.BuildServiceProvider();

        var typeFeatureProvider = _sp.GetRequiredService<ITypeFeatureProvider>();
        typeFeatureProvider.TryAdd(typeof(MultiBindingTestShapeProvider), A1);

        MultiBindingTestShapeProvider.Init(A1, A2, B1);
    }

    [Fact]
    public async Task OnlyFirstBindingPerBindingSourceIsUsed()
    {
        var manager = _sp.GetRequiredService<IShapeTableManager>();
        var table = await manager.GetShapeTableAsync(themeId: null);

        var descriptor = table.Descriptors["TestShape"];
        var binding = table.Bindings["TestShape"];

        // Expected: Override from ModuleB should win.
        Assert.Equal("OverrideSource", descriptor.BindingSource);
        Assert.Equal("OverrideSource", binding.BindingSource);
    }

    public void Dispose()
    {
        (_sp as IDisposable)?.Dispose();
    }
}
