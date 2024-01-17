using OrchardCore.DisplayManagement.Events;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Modules;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Extensions
{
    public class ExtensionManagerTests
    {
        private static readonly IHostEnvironment _hostingEnvironment
            = new StubHostingEnvironment();

        private static readonly IApplicationContext _applicationContext
            = new ModularApplicationContext(_hostingEnvironment, new List<IModuleNamesProvider>()
            {
                new ModuleNamesProvider()
            });

        private static readonly IFeaturesProvider _moduleFeatureProvider =
            new FeaturesProvider(new[] { new ThemeFeatureBuilderEvents() });

        private static readonly IFeaturesProvider _themeFeatureProvider =
            new FeaturesProvider(new[] { new ThemeFeatureBuilderEvents() });

        private readonly IExtensionManager _moduleScopedExtensionManager;
        private readonly IExtensionManager _themeScopedExtensionManager;
        private readonly IExtensionManager _moduleThemeScopedExtensionManager;

        public ExtensionManagerTests()
        {
            _moduleScopedExtensionManager = new ExtensionManager(
                _applicationContext,
                new[] { new ExtensionDependencyStrategy() },
                new[] { new ExtensionPriorityStrategy() },
                new TypeFeatureProvider(),
                _moduleFeatureProvider,
                new NullLogger<ExtensionManager>()
                );

            _themeScopedExtensionManager = new ExtensionManager(
                _applicationContext,
                new[] { new ExtensionDependencyStrategy() },
                new[] { new ExtensionPriorityStrategy() },
                new TypeFeatureProvider(),
                _themeFeatureProvider,
                new NullLogger<ExtensionManager>()
                );

            _moduleThemeScopedExtensionManager = new ExtensionManager(
                _applicationContext,
                new IExtensionDependencyStrategy[] { new ExtensionDependencyStrategy(), new ThemeExtensionDependencyStrategy() },
                new[] { new ExtensionPriorityStrategy() },
                new TypeFeatureProvider(),
                _themeFeatureProvider,
                new NullLogger<ExtensionManager>()
                );
        }

        private class ModuleNamesProvider : IModuleNamesProvider
        {
            private readonly string[] _moduleNames;

            public ModuleNamesProvider()
            {
                _moduleNames = new[]
                {
                    "BaseThemeSample",
                    "BaseThemeSample2",
                    "DerivedThemeSample",
                    "DerivedThemeSample2",
                    "ModuleSample"
                };
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
            var features = _moduleScopedExtensionManager.GetFeatures(new[] { "Sample2" });

            Assert.Equal(2, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
        }

        [Fact]
        public void GetFeaturesWithAIdShouldReturnThatFeatureWithDependenciesOrderedWithNoDuplicates()
        {
            var features = _moduleScopedExtensionManager.GetFeatures(new[] { "Sample2", "Sample3" });

            Assert.Equal(3, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
            Assert.Equal("Sample3", features.ElementAt(2).Id);
        }

        [Fact]
        public void GetFeaturesWithAIdShouldNotReturnFeaturesTheHaveADependencyOutsideOfGraph()
        {
            var features = _moduleScopedExtensionManager.GetFeatures(new[] { "Sample4" });

            Assert.Equal(3, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
            Assert.Equal("Sample4", features.ElementAt(2).Id);
        }

        /* Theme Base Theme Dependencies */

        [Fact]
        public void GetFeaturesShouldReturnCorrectThemeHeirarchy()
        {
            var features = _themeScopedExtensionManager.GetFeatures(new[] { "DerivedThemeSample" });

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
            var features = _moduleThemeScopedExtensionManager.GetFeatures(new[] { "DerivedThemeSample", "Sample3" });

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
    }
}
