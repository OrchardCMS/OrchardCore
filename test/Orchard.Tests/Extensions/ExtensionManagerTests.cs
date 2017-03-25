using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Orchard.DisplayManagement.Events;
using Orchard.DisplayManagement.Extensions;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Tests.Stubs;
using Xunit;

namespace Orchard.Tests.Extensions
{
    public class ExtensionManagerTests
    {
        private static IHostingEnvironment HostingEnvironment
            = new StubHostingEnvironment(Path.Combine(
                PlatformServices.Default.Application.ApplicationBasePath, "Extensions"));

        private static IOptions<ManifestOptions> ModuleManifestOptions =
            new StubManifestOptions(
                new ManifestOption { ManifestFileName = "Module.txt", Type = "module" }
                );

        private static IOptions<ManifestOptions> ThemeManifestOptions =
            new StubManifestOptions(
                new ManifestOption { ManifestFileName = "Theme.txt", Type = "theme" }
                );

        private static IOptions<ManifestOptions> ModuleAndThemeManifestOptions =
            new StubManifestOptions(
                new ManifestOption { ManifestFileName = "Module.txt", Type = "module" },
                new ManifestOption { ManifestFileName = "Theme.txt", Type = "theme" }
                );

        private static IEnumerable<IManifestProvider> ManifestProviders =
            new[] { new ManifestProvider(HostingEnvironment) };

        private static IExtensionProvider ModuleProvider
            = new ExtensionProvider(
                HostingEnvironment,
                new[] { new FeaturesProvider(Enumerable.Empty<IFeatureBuilderEvents>(), new NullLogger<FeaturesProvider>()) });

        private static IExtensionProvider ThemeProvider
            = new ExtensionProvider(
                HostingEnvironment,
                new[] { new FeaturesProvider(new[] { new ThemeFeatureBuilderEvents() }, new NullLogger<FeaturesProvider>()) });

        private static IOptions<ExtensionExpanderOptions> ExtensionExpanderOptions =
            new StubExtensionExpanderOptions(
                new ExtensionExpanderOption { SearchPath = "TestDependencyModules" });

        private IExtensionManager ModuleScopedExtensionManager;
        private IExtensionManager ThemeScopedExtensionManager;
        private IExtensionManager ModuleThemeScopedExtensionManager;

        public ExtensionManagerTests()
        {
            ModuleScopedExtensionManager = new ExtensionManager(
                ExtensionExpanderOptions,
                ModuleManifestOptions,
                HostingEnvironment,
                ManifestProviders,
                new[] { ModuleProvider },
                Enumerable.Empty<IExtensionLoader>(),
                new[] { new ExtensionDependencyStrategy() },
                new[] { new ExtensionPriorityStrategy() },
                null,
                new NullLogger<ExtensionManager>(),
                null);

            ThemeScopedExtensionManager = new ExtensionManager(
                ExtensionExpanderOptions,
                ThemeManifestOptions,
                HostingEnvironment,
                ManifestProviders,
                new[] { ThemeProvider },
                Enumerable.Empty<IExtensionLoader>(),
                new[] { new ExtensionDependencyStrategy() },
                new[] { new ExtensionPriorityStrategy() },
                null,
                new NullLogger<ExtensionManager>(),
                null);

            ModuleThemeScopedExtensionManager = new ExtensionManager(
                ExtensionExpanderOptions,
                ModuleAndThemeManifestOptions,
                HostingEnvironment,
                ManifestProviders,
                new[] { ThemeProvider, ModuleProvider },
                Enumerable.Empty<IExtensionLoader>(),
                new IExtensionDependencyStrategy[] { new ExtensionDependencyStrategy(), new ThemeExtensionDependencyStrategy() },
                new[] { new ExtensionPriorityStrategy() },
                null,
                new NullLogger<ExtensionManager>(),
                null);
        }

        [Fact]
        public void ShouldReturnExtension()
        {
            var extensions = ModuleScopedExtensionManager.GetExtensions();

            Assert.Equal(4, extensions.Count());
        }

        [Fact]
        public void ShouldReturnAllDependenciesIncludingFeatureForAGivenFeatureOrdered()
        {
            var features = ModuleScopedExtensionManager.GetFeatureDependencies("Sample3");

            Assert.Equal(3, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
            Assert.Equal("Sample3", features.ElementAt(2).Id);
        }

        [Fact]
        public void ShouldNotReturnFeaturesNotDependentOn()
        {
            var features = ModuleScopedExtensionManager.GetFeatureDependencies("Sample2");

            Assert.Equal(2, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
        }

        [Fact]
        public void GetDependentFeaturesShouldReturnAllFeaturesThatHaveADependencyOnAFeature()
        {
            var features = ModuleScopedExtensionManager.GetDependentFeatures("Sample1");

            Assert.Equal(4, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
            Assert.Equal("Sample3", features.ElementAt(2).Id);
            Assert.Equal("Sample4", features.ElementAt(3).Id);
        }

        [Fact]
        public void GetFeaturesShouldReturnAllFeaturesOrderedByDependency()
        {
            var features = ModuleScopedExtensionManager.GetFeatures();

            Assert.Equal(4, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
            Assert.Equal("Sample3", features.ElementAt(2).Id);
            Assert.Equal("Sample4", features.ElementAt(3).Id);
        }

        [Fact]
        public void GetFeaturesWithAIdShouldReturnThatFeatureWithDependenciesOrdered()
        {
            var features = ModuleScopedExtensionManager.GetFeatures(new[] { "Sample2" });

            Assert.Equal(2, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
        }


        [Fact]
        public void GetFeaturesWithAIdShouldReturnThatFeatureWithDependenciesOrderedWithNoDuplicates()
        {
            var features = ModuleScopedExtensionManager.GetFeatures(new[] { "Sample2", "Sample3" });

            Assert.Equal(3, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
            Assert.Equal("Sample3", features.ElementAt(2).Id);
        }

        [Fact]
        public void GetFeaturesWithAIdShouldNotReturnFeaturesTheHaveADependencyOutsideOfGraph()
        {
            var features = ModuleScopedExtensionManager.GetFeatures(new[] { "Sample4" });

            Assert.Equal(3, features.Count());
            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
            Assert.Equal("Sample4", features.ElementAt(2).Id);
        }

        /* Theme Base Theme Dependencies */

        [Fact]
        public void GetFeaturesShouldReturnCorrectThemeHeirarchy()
        {
            var features = ThemeScopedExtensionManager.GetFeatures(new[] { "DerivedThemeSample" });

            Assert.Equal(2, features.Count());
            Assert.Equal("BaseThemeSample", features.ElementAt(0).Id);
            Assert.Equal("DerivedThemeSample", features.ElementAt(1).Id);
        }

        /* Theme and Module Dependencies */

        [Fact]
        public void GetFeaturesShouldReturnBothThemesAndModules()
        {
            var features = ModuleThemeScopedExtensionManager.GetFeatures();

            Assert.Equal(8, features.Count());
        }

        [Fact]
        public void GetFeaturesShouldReturnThemesAfterModules()
        {
            var features = ModuleThemeScopedExtensionManager.GetFeatures();

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
            var features = ModuleThemeScopedExtensionManager.GetFeatures(new[] { "DerivedThemeSample", "Sample3" });

            Assert.Equal("Sample1", features.ElementAt(0).Id);
            Assert.Equal("Sample2", features.ElementAt(1).Id);
            Assert.Equal("Sample3", features.ElementAt(2).Id);
            Assert.Equal("BaseThemeSample", features.ElementAt(3).Id);
            Assert.Equal("DerivedThemeSample", features.ElementAt(4).Id);
        }

        [Fact]
        public void ShouldReturnNotFoundExtensionInfoWhenNotFound()
        {
            var extension = ModuleThemeScopedExtensionManager.GetExtension("NotFound");

            Assert.False(extension.Exists);
        }
    }
}