using System;
using System.Linq;
using OrchardCore.ResourceManagement;
using Xunit;

namespace OrchardCore.Tests.SourceGenerators
{
    public class ResourceManfiestGeneratorTests
    {
        [Fact]
        public void ResourceManifestGeneratorShouldGenerateResources()
        {
            // Arrange & Act
            var resourceManifest = ResourceManfiestGenerator.Build(String.Empty);

            // Assert
            var styles = resourceManifest.GetResources("stylesheet");
            var scripts = resourceManifest.GetResources("script");

            Assert.True(styles.Count > 0);
            Assert.True(scripts.Count > 0);
        }

        [Fact]
        public void MonacoLoaderShouldBeLastScript()
        {
            // Arrange & Act
            var resourceManifest = ResourceManfiestGenerator.Build(String.Empty);

            // Assert
            var lastScript = resourceManifest.GetResources("script").Values
                .SelectMany(r => r)
                .Single(r => r.Position == ResourcePosition.Last);

            Assert.Equal("monaco-loader", lastScript.Name);
        }

        [Fact]
        public void ResourceManifestGeneratorShouldMinifyUrlAndCdnIfNeeded()
        {
            // Arrange
            const string minificationFileExtension = ".min";
            var resourceManifest = ResourceManfiestGenerator.Build(String.Empty);

            // Act & Assert
            var resources = resourceManifest.GetResources("script").Values
                .SelectMany(r => r)
                .Where(r => !String.IsNullOrEmpty(r.Url));

            var resourcesWithUrlDebugVersion = resources.Where(r => r.Url.Contains(minificationFileExtension));
            var resourcesWithoutUrlDebugVersion = resources.Where(r => !r.Url.Contains(minificationFileExtension));

            Assert.Equal(resourcesWithUrlDebugVersion.Select(r => r.Url.Replace(minificationFileExtension, String.Empty)), resourcesWithUrlDebugVersion.Select(r => r.UrlDebug));
            Assert.True(resourcesWithoutUrlDebugVersion.All(r => String.IsNullOrEmpty(r.UrlDebug)));
        }
    }
}
