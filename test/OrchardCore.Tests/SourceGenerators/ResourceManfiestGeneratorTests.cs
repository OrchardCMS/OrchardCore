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
    }
}
