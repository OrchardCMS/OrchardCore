using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Events;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Tests.Stubs;
using System.IO;
using System.Linq;
using Xunit;

namespace Orchard.Tests.Extensions
{
    public class ExtensionManagerTests
    {
        private static IFileProvider RunningTestFileProvider
            = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Extensions"));

        private static IHostingEnvironment HostingEnvrionment
            = new StubHostingEnvironment { ContentRootFileProvider = RunningTestFileProvider };

        private static IExtensionProvider ModuleProvider =
                    new ExtensionProvider(
                        HostingEnvrionment,
                        new ManifestBuilder(
                            new IManifestProvider[] { new ManifestProvider(HostingEnvrionment) },
                            HostingEnvrionment,
                            new StubManifestOptions(new ManifestOption { ManifestFileName = "Module.txt", Type = "module" })),
                        new[] { new FeaturesProvider(Enumerable.Empty<IFeatureBuilderEvents>(), new NullLogger<FeaturesProvider>()) });

        private static IExtensionProvider ThemeProvider =
            new ExtensionProvider(
                HostingEnvrionment,
                new ManifestBuilder(
                    new IManifestProvider[] { new ManifestProvider(HostingEnvrionment) },
                    HostingEnvrionment,
                    new StubManifestOptions(new ManifestOption { ManifestFileName = "Theme.txt", Type = "theme" })),
                new[] { new FeaturesProvider(new[] { new ThemeFeatureBuilderEvents() }, new NullLogger<FeaturesProvider>()) });


        private IExtensionManager ModuleScopedExtensionManager;
        private IExtensionManager ThemeScopedExtensionManager;

        public ExtensionManagerTests() {
            ModuleScopedExtensionManager = new ExtensionManager(
                new StubExtensionOptions("TestDependencyModules"),
                new[] { ModuleProvider },
                Enumerable.Empty<IExtensionLoader>(),
                HostingEnvrionment,
                null,
                new NullLogger<ExtensionManager>(),
                null);

            ThemeScopedExtensionManager = new ExtensionManager(
                new StubExtensionOptions("TestDependencyModules"),
                new[] { ThemeProvider },
                Enumerable.Empty<IExtensionLoader>(),
                HostingEnvrionment,
                null,
                new NullLogger<ExtensionManager>(),
                null);
        }

        [Fact]

        public void ShouldReturnExtension() {
            var extensions = ModuleScopedExtensionManager.GetExtensions();

            Assert.Equal(6, extensions.Count());
        }

        [Fact]
        public void ShouldReturnAllDependenciesIncludingFeatureForAGivenFeatureOrdered() {
            var features = ModuleScopedExtensionManager.GetFeatureDependencies("Sample3");

            Assert.Equal(3, features.Count());
            Assert.Equal(features.ElementAt(0).Id, "Sample1");
            Assert.Equal(features.ElementAt(1).Id, "Sample2");
            Assert.Equal(features.ElementAt(2).Id, "Sample3");
        }

        [Fact]
        public void ShouldNotReturnFeaturesNotDependentOn() {
            var features = ModuleScopedExtensionManager.GetFeatureDependencies("Sample2");

            Assert.Equal(2, features.Count());
            Assert.Equal(features.ElementAt(0).Id, "Sample1");
            Assert.Equal(features.ElementAt(1).Id, "Sample2");
        }

        [Fact]
        public void GetDependentFeaturesShouldReturnAllFeaturesThatHaveADependencyOnAFeature() {
            var features = ModuleScopedExtensionManager.GetDependentFeatures("Sample1");

            Assert.Equal(4, features.Count());
            Assert.Equal(features.ElementAt(0).Id, "Sample1");
            Assert.Equal(features.ElementAt(1).Id, "Sample2");
            Assert.Equal(features.ElementAt(2).Id, "Sample3");
            Assert.Equal(features.ElementAt(3).Id, "Sample4");
        }

        [Fact]
        public void GetFeaturesShouldReturnAllFeaturesOrderedByDependency() {
            var features = ModuleScopedExtensionManager.GetFeatures();

            Assert.Equal(4, features.Count());
            Assert.Equal(features.ElementAt(0).Id, "Sample1");
            Assert.Equal(features.ElementAt(1).Id, "Sample2");
            Assert.Equal(features.ElementAt(2).Id, "Sample3");
            Assert.Equal(features.ElementAt(3).Id, "Sample4");
        }

        [Fact]
        public void GetFeaturesWithAIdShouldReturnThatFeatureWithDependenciesOrdered() {
            var features = ModuleScopedExtensionManager.GetFeatures(new[] { "Sample2" });

            Assert.Equal(2, features.Count());
            Assert.Equal(features.ElementAt(0).Id, "Sample1");
            Assert.Equal(features.ElementAt(1).Id, "Sample2");
        }


        [Fact]
        public void GetFeaturesWithAIdShouldReturnThatFeatureWithDependenciesOrderedWithNoDuplicates()
        {
            var features = ModuleScopedExtensionManager.GetFeatures(new[] { "Sample2", "Sample3" });

            Assert.Equal(3, features.Count());
            Assert.Equal(features.ElementAt(0).Id, "Sample1");
            Assert.Equal(features.ElementAt(1).Id, "Sample2");
            Assert.Equal(features.ElementAt(2).Id, "Sample3");
        }

        [Fact]
        public void GetFeaturesWithAIdShouldNotReturnFeaturesTheHaveADependencyOutsideOfGraph()
        {
            var features = ModuleScopedExtensionManager.GetFeatures(new[] { "Sample4" });

            Assert.Equal(3, features.Count());
            Assert.Equal(features.ElementAt(0).Id, "Sample1");
            Assert.Equal(features.ElementAt(1).Id, "Sample2");
            Assert.Equal(features.ElementAt(2).Id, "Sample4");
        }

        /* Theme Base Theme Dependencies */

        [Fact]
        public void GetFeaturesShouldReturnCorrectThemeHeirarchy()
        {
            var features = ThemeScopedExtensionManager.GetFeatures(new[] { "DerivedThemeSample" });

            Assert.Equal(2, features.Count());
            Assert.Equal(features.ElementAt(0).Id, "BaseThemeSample");
            Assert.Equal(features.ElementAt(1).Id, "DerivedThemeSample");
        }

    }

    public class StubExtensionOptions : IOptions<ExtensionOptions>
    {
        private string _path;
        public StubExtensionOptions(string path)
        {
            _path = path;
        }

        public ExtensionOptions Value
        {
            get
            {
                var options = new ExtensionOptions();
                options.SearchPaths.Add(_path);
                return options;
            }
        }
    }
}
