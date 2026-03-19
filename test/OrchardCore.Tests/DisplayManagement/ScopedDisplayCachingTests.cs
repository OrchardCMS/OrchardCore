using Microsoft.Extensions.FileProviders;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules.Manifest;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement;

public class ScopedDisplayCachingTests
{
    [Fact]
    public async Task GetThemeAsync_CachesInFlightTask()
    {
        var selector = new BlockingThemeSelector();
        var themeManager = new ThemeManager([selector], new TestExtensionManager(new TestExtensionInfo("Theme")));

        var firstThemeTask = themeManager.GetThemeAsync();
        await selector.WaitUntilCalledAsync();

        var secondThemeTask = themeManager.GetThemeAsync();

        Assert.Equal(1, selector.CallCount);

        selector.SetResult(new ThemeSelectorResult { Priority = 1, ThemeName = "Theme" });

        var themes = await Task.WhenAll(firstThemeTask, secondThemeTask);

        Assert.All(themes, theme => Assert.NotNull(theme));
        Assert.All(themes, theme => Assert.Equal("Theme", theme.Id));
        Assert.Same(themes[0], themes[1]);
    }

    [Fact]
    public async Task GetThemeAsync_DoesNotCacheNullTheme()
    {
        var selector = new SequenceThemeSelector(
        [
            null,
            new ThemeSelectorResult { Priority = 1, ThemeName = "Theme" },
        ]);

        var themeManager = new ThemeManager([selector], new TestExtensionManager(new TestExtensionInfo("Theme")));

        Assert.Null(await themeManager.GetThemeAsync());

        var theme = await themeManager.GetThemeAsync();

        Assert.NotNull(theme);
        Assert.Equal("Theme", theme.Id);
        Assert.Equal(2, selector.CallCount);
    }

    [Fact]
    public async Task GetLayoutAsync_CachesInFlightTask()
    {
        var shapeFactory = new BlockingShapeFactory();
        var layoutAccessor = new LayoutAccessor(shapeFactory);

        var firstLayoutTask = layoutAccessor.GetLayoutAsync();
        await shapeFactory.WaitUntilCalledAsync();

        var secondLayoutTask = layoutAccessor.GetLayoutAsync();

        Assert.Equal(1, shapeFactory.CallCount);

        var layout = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        shapeFactory.SetResult(layout);

        var layouts = await Task.WhenAll(firstLayoutTask, secondLayoutTask);

        Assert.All(layouts, resolvedLayout => Assert.Same(layout, resolvedLayout));
    }

    [Fact]
    public async Task BuildPlacementInfoResolverAsync_ReturnsSameResolverAcrossConcurrentCalls()
    {
        var shapeTable = new ShapeTable(
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase));
        var themeManager = new BlockingThemeManager();
        var provider = new ShapeTablePlacementProvider(new TestShapeTableManager(shapeTable), themeManager);
        var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var resolverTasks = Enumerable.Range(0, 32).Select(_ => Task.Run(async () =>
        {
            await start.Task;
            return await provider.BuildPlacementInfoResolverAsync(TestBuildShapeContext.Instance);
        })).ToArray();

        start.SetResult(true);
        themeManager.SetResult(new TestExtensionInfo("Theme"));

        var resolvers = await Task.WhenAll(resolverTasks);

        Assert.All(resolvers, resolver => Assert.NotNull(resolver));
        Assert.All(resolvers, resolver => Assert.Same(resolvers[0], resolver));
    }

    private sealed class BlockingThemeSelector : IThemeSelector
    {
        private readonly TaskCompletionSource<ThemeSelectorResult> _resultSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _calledSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _callCount;

        public int CallCount => _callCount;

        public Task<ThemeSelectorResult> GetThemeAsync()
        {
            Interlocked.Increment(ref _callCount);
            _calledSource.TrySetResult(true);

            return _resultSource.Task;
        }

        public Task WaitUntilCalledAsync() => _calledSource.Task;

        public void SetResult(ThemeSelectorResult result) => _resultSource.TrySetResult(result);
    }

    private sealed class SequenceThemeSelector(IEnumerable<ThemeSelectorResult?> results) : IThemeSelector
    {
        private readonly Queue<ThemeSelectorResult?> _results = new(results);
        private int _callCount;

        public int CallCount => _callCount;

        public Task<ThemeSelectorResult> GetThemeAsync()
        {
            _callCount++;
            return Task.FromResult(_results.Dequeue()!);
        }
    }

    private sealed class BlockingThemeManager : IThemeManager
    {
        private readonly TaskCompletionSource<IExtensionInfo> _themeSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<IExtensionInfo> GetThemeAsync() => _themeSource.Task;

        public void SetResult(IExtensionInfo theme) => _themeSource.TrySetResult(theme);
    }

    private sealed class BlockingShapeFactory : IShapeFactory
    {
        private readonly TaskCompletionSource<IShape> _shapeSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _calledSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _callCount;

        public int CallCount => _callCount;
        public dynamic New => throw new NotSupportedException();

        public async ValueTask<IShape> CreateAsync(string shapeType, Func<ValueTask<IShape>> shapeFactory, Action<ShapeCreatingContext> creating, Action<ShapeCreatedContext> created)
        {
            Interlocked.Increment(ref _callCount);
            _calledSource.TrySetResult(true);

            return await _shapeSource.Task;
        }

        public Task WaitUntilCalledAsync() => _calledSource.Task;

        public void SetResult(IShape shape) => _shapeSource.TrySetResult(shape);
    }

    private sealed class TestExtensionManager(IExtensionInfo extensionInfo) : IExtensionManager
    {
        public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId) => throw new NotImplementedException();

        public IExtensionInfo GetExtension(string extensionId)
        {
            return extensionId == extensionInfo.Id ? extensionInfo : throw new InvalidOperationException();
        }

        public IEnumerable<IExtensionInfo> GetExtensions() => [extensionInfo];

        public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId) => throw new NotImplementedException();

        public IEnumerable<IFeatureInfo> GetFeatures() => [];

        public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad) => [];

        public IEnumerable<IFeatureInfo> GetFeatures(IEnumerable<string> featureIdsToLoad) => [];

        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo) => throw new NotImplementedException();

        public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync() => throw new NotImplementedException();

        public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(string[] featureIdsToLoad) => throw new NotImplementedException();

        public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(IEnumerable<string> featureIdsToLoad) => throw new NotImplementedException();
    }

    private sealed class TestExtensionInfo : IExtensionInfo
    {
        public TestExtensionInfo(string id)
        {
            Id = id;
            Manifest = new ManifestInfo(new ThemeAttribute());
            Features = [new FeatureInfo(id, this)];
            SubPath = id;
        }

        public IFileInfo ExtensionFileInfo { get; set; }
        public IEnumerable<IFeatureInfo> Features { get; set; }
        public string Id { get; set; }
        public IManifestInfo Manifest { get; set; }
        public string SubPath { get; set; }
        public bool Exists => true;
    }

    private sealed class TestBuildShapeContext : IBuildShapeContext
    {
        public static readonly TestBuildShapeContext Instance = new();

        public IShape Shape => new Shape();
        public IShapeFactory ShapeFactory => throw new NotSupportedException();
        public dynamic New => throw new NotSupportedException();
        public IZoneHolding Layout { get; set; } = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        public string GroupId => string.Empty;
        public FindPlacementDelegate FindPlacement { get; set; } = static (_, _, _, _) => null;
    }
}
