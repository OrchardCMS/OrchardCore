using BaseThemeSample;
using ModuleSample;
using OrchardCore.DisplayManagement.Events;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Modules;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Extensions;

public class ExtensionManagerTests
{
    private static readonly IHostEnvironment _hostingEnvironment
        = new StubHostingEnvironment();

    private static readonly IApplicationContext _applicationContext
        = new ModularApplicationContext(_hostingEnvironment, [new ModuleNamesProvider()]);

    private static readonly IFeaturesProvider _moduleFeatureProvider =
        new FeaturesProvider(new[] { new ThemeFeatureBuilderEvents() });

    private static readonly IFeaturesProvider _themeFeatureProvider =
        new FeaturesProvider(new[] { new ThemeFeatureBuilderEvents() });

    private readonly ExtensionManager _moduleScopedExtensionManager;
    private readonly ExtensionManager _themeScopedExtensionManager;
    private readonly ExtensionManager _moduleThemeScopedExtensionManager;

    private readonly TypeFeatureProvider _moduleScopedTypeFeatureProvider = new TypeFeatureProvider();

    public ExtensionManagerTests()
    {
        _moduleScopedExtensionManager = CreateExtensionManager(
            [new ExtensionDependencyStrategy()],
            [new ExtensionPriorityStrategy()],
            _moduleScopedTypeFeatureProvider,
            _moduleFeatureProvider
            );

        _themeScopedExtensionManager = CreateExtensionManager(
            [new ExtensionDependencyStrategy()],
            [new ExtensionPriorityStrategy()],
            new TypeFeatureProvider(),
            _themeFeatureProvider
            );

        _moduleThemeScopedExtensionManager = CreateExtensionManager(
            [new ExtensionDependencyStrategy(), new ThemeExtensionDependencyStrategy()],
            [new ExtensionPriorityStrategy()],
            new TypeFeatureProvider(),
            _themeFeatureProvider
            );
    }

    private sealed class ModuleNamesProvider : IModuleNamesProvider
    {
        private readonly string[] _moduleNames;

        public ModuleNamesProvider()
        {
            _moduleNames =
            [
                "BaseThemeSample",
                "BaseThemeSample2",
                "DerivedThemeSample",
                "DerivedThemeSample2",
                "ModuleSample"
            ];
        }

        public IEnumerable<string> GetModuleNames()
        {
            return _moduleNames;
        }
    }

    [Fact]
    public void ShouldReturnExtension()
    {
        var extensions = _moduleThemeScopedExtensionManager.GetExtensions()
            .Where(e => e.Manifest.ModuleInfo.Category == "Test");

        Assert.Equal(5, extensions.Count());
    }

    [Fact]
    public void ShouldReturnAllDependenciesIncludingFeatureForAGivenFeatureOrdered()
    {
        var features = _moduleScopedExtensionManager.GetFeatureDependencies("Sample3");

        Assert.Equal(3, features.Count());
        Assert.Equal("Sample1", features.ElementAt(0).Id);
        Assert.Equal("Sample2", features.ElementAt(1).Id);
        Assert.Equal("Sample3", features.ElementAt(2).Id);
    }

    [Fact]
    public void ShouldNotReturnFeaturesNotDependentOn()
    {
        var features = _moduleScopedExtensionManager.GetFeatureDependencies("Sample2");

        Assert.Equal(2, features.Count());
        Assert.Equal("Sample1", features.ElementAt(0).Id);
        Assert.Equal("Sample2", features.ElementAt(1).Id);
    }

    [Fact]
    public void GetDependentFeaturesShouldReturnAllFeaturesThatHaveADependencyOnAFeature()
    {
        var features = _moduleScopedExtensionManager.GetDependentFeatures("Sample1");

        Assert.Equal(4, features.Count());
        Assert.Equal("Sample1", features.ElementAt(0).Id);
        Assert.Equal("Sample2", features.ElementAt(1).Id);
        Assert.Equal("Sample3", features.ElementAt(2).Id);
        Assert.Equal("Sample4", features.ElementAt(3).Id);
    }

    [Fact]
    public void GetFeaturesShouldReturnAllFeaturesOrderedByDependency()
    {
        var features = _moduleScopedExtensionManager.GetFeatures()
            .Where(f => f.Category == "Test" && !f.IsTheme());

        Assert.Equal(4, features.Count());
        Assert.Equal("Sample1", features.ElementAt(0).Id);
        Assert.Equal("Sample2", features.ElementAt(1).Id);
        Assert.Equal("Sample3", features.ElementAt(2).Id);
        Assert.Equal("Sample4", features.ElementAt(3).Id);
    }

    [Fact]
    public void GetFeaturesWithAIdShouldReturnThatFeatureWithDependenciesOrdered()
    {
        var features = _moduleScopedExtensionManager.GetFeatures(["Sample2"]);

        Assert.Equal(2, features.Count());
        Assert.Equal("Sample1", features.ElementAt(0).Id);
        Assert.Equal("Sample2", features.ElementAt(1).Id);
    }

    [Fact]
    public void GetFeaturesWithAIdShouldReturnThatFeatureWithDependenciesOrderedWithNoDuplicates()
    {
        var features = _moduleScopedExtensionManager.GetFeatures(["Sample2", "Sample3"]);

        Assert.Equal(3, features.Count());
        Assert.Equal("Sample1", features.ElementAt(0).Id);
        Assert.Equal("Sample2", features.ElementAt(1).Id);
        Assert.Equal("Sample3", features.ElementAt(2).Id);
    }

    [Fact]
    public void GetFeaturesWithAIdShouldNotReturnFeaturesTheHaveADependencyOutsideOfGraph()
    {
        var features = _moduleScopedExtensionManager.GetFeatures(["Sample4"]);

        Assert.Equal(3, features.Count());
        Assert.Equal("Sample1", features.ElementAt(0).Id);
        Assert.Equal("Sample2", features.ElementAt(1).Id);
        Assert.Equal("Sample4", features.ElementAt(2).Id);
    }

    /* Theme Base Theme Dependencies */

    [Fact]
    public void GetFeaturesShouldReturnCorrectThemeHierarchy()
    {
        var features = _themeScopedExtensionManager.GetFeatures(["DerivedThemeSample"]);

        Assert.Equal(2, features.Count());
        Assert.Equal("BaseThemeSample", features.ElementAt(0).Id);
        Assert.Equal("DerivedThemeSample", features.ElementAt(1).Id);
    }

    /* Theme and Module Dependencies */

    [Fact]
    public void GetFeaturesShouldReturnBothThemesAndModules()
    {
        var features = _moduleThemeScopedExtensionManager.GetFeatures()
            .Where(f => f.Category == "Test");

        Assert.Equal(8, features.Count());
    }

    [Fact]
    public void GetFeaturesShouldReturnThemesAfterModules()
    {
        var features = _moduleThemeScopedExtensionManager.GetFeatures()
            .Where(f => f.Category == "Test");

        Assert.Equal("Sample1", features.ElementAt(0).Id);
        Assert.Equal("Sample2", features.ElementAt(1).Id);
        Assert.Equal("Sample3", features.ElementAt(2).Id);
        Assert.Equal("Sample4", features.ElementAt(3).Id);
        Assert.Equal("BaseThemeSample", features.ElementAt(4).Id);
        Assert.Equal("BaseThemeSample2", features.ElementAt(5).Id);
        Assert.Equal("DerivedThemeSample", features.ElementAt(6).Id);
        Assert.Equal("DerivedThemeSample2", features.ElementAt(7).Id);
    }

    [Fact]
    public void GetFeaturesShouldReturnThemesAfterModulesWhenRequestingBoth()
    {
        var features = _moduleThemeScopedExtensionManager.GetFeatures(["DerivedThemeSample", "Sample3"]);

        Assert.Equal("Sample1", features.ElementAt(0).Id);
        Assert.Equal("Sample2", features.ElementAt(1).Id);
        Assert.Equal("Sample3", features.ElementAt(2).Id);
        Assert.Equal("BaseThemeSample", features.ElementAt(3).Id);
        Assert.Equal("DerivedThemeSample", features.ElementAt(4).Id);
    }

    [Fact]
    public void ShouldReturnNotFoundExtensionInfoWhenNotFound()
    {
        var extension = _moduleThemeScopedExtensionManager.GetExtension("NotFound");

        Assert.False(extension.Exists);
    }

    /* The extension manager must populate the ITypeFeatureProvider correctly */

    [Fact]
    public void TypeFeatureProviderIsPopulatedWithComponentTypes()
    {
        var feature = _moduleScopedExtensionManager.GetFeatures(["Sample1"]).First();
        var types = _moduleScopedTypeFeatureProvider.GetTypesForFeature(feature);

        Assert.Equal(2, types.Count());
        Assert.Contains(typeof(Sample1Startup), types);
        Assert.Contains(typeof(FeatureIndependentStartup), types);
    }

    [Fact]
    public void TypeFeatureProviderTypeMustBeMappedToAllFeatures()
    {
        // Types in modules that have no feature that matches the extension ID must be mapped to all features.
        var features = _moduleScopedExtensionManager.GetFeatures(["Sample1", "Sample2", "Sample3", "Sample4"]);

        foreach (var feature in features)
        {
            var types = _moduleScopedTypeFeatureProvider.GetTypesForFeature(feature);

            Assert.Contains(typeof(FeatureIndependentStartup), types);
        }
    }

    [Fact]
    public void TypeFeatureProviderTypeMustBeMappedToExtensionFeature()
    {
        // Types in modules that have a feature that matches the extension ID must be mapped to that feature.
        var feature = _moduleScopedExtensionManager.GetFeatures(["BaseThemeSample"]).First();
        var types = _moduleScopedTypeFeatureProvider.GetTypesForFeature(feature);

        Assert.Equal(2, types.Count());
        Assert.Contains(typeof(BaseThemeSampleStartup), types);
        Assert.Contains(typeof(BaseThemeFeatureIndependentStartup), types);
    }

    [Fact]
    public void TypeFeatureProviderTypeMustBeSkipped()
    {
        var feature = _moduleScopedExtensionManager.GetFeatures(["Sample2"]).First();
        var types = _moduleScopedTypeFeatureProvider.GetTypesForFeature(feature);

        Assert.DoesNotContain(typeof(SkippedDependentType), types);
    }

    private static ExtensionManager CreateExtensionManager(
        IExtensionDependencyStrategy[] extensionDependencyStrategies,
        IExtensionPriorityStrategy[] extensionPriorityStrategies,
        ITypeFeatureProvider typeFeatureProvider,
        IFeaturesProvider featuresProvider)
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(_applicationContext)
            .AddSingleton(typeFeatureProvider)
            .AddSingleton(featuresProvider);

        foreach (var extensionDependencyStrategy in extensionDependencyStrategies)
        {
            services.AddSingleton(extensionDependencyStrategy);
        }

        foreach (var extensionPriorityStrategy in extensionPriorityStrategies)
        {
            services.AddSingleton(extensionPriorityStrategy);
        }

        var serviceProvider = services.BuildServiceProvider();

        return new ExtensionManager(serviceProvider, new NullLogger<ExtensionManager>());
    }
}
