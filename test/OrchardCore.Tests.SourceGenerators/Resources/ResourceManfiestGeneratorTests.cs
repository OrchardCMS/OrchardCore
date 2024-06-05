namespace OrchardCore.SourceGenerators.Resouces.Tests;

public class ResourceManfiestGeneratorTests
{
    [Fact]
    public void ResourceManifestGeneratorShouldGenerateResources()
    {
        // Arrange & Act
        var resourceManifest = ResourceManifestGenerator.Build(string.Empty);

        // Assert
        var styles = resourceManifest.GetResources("stylesheet");
        var scripts = resourceManifest.GetResources("script");

        Assert.NotEmpty(styles);
        Assert.NotEmpty(scripts);

        Assert.Equal(13, styles.Count);
    }

    [Fact]
    public void MonacoLoaderShouldBeLastScript()
    {
        // Arrange & Act
        var resourceManifest = ResourceManifestGenerator.Build(string.Empty);

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
        var resourceManifest = ResourceManifestGenerator.Build(string.Empty);

        // Act & Assert
        var resources = resourceManifest.GetResources("script").Values
            .SelectMany(r => r)
            .Where(r => !string.IsNullOrEmpty(r.Url));

        var resourcesWithUrlDebugVersion = resources.Where(r => r.Url.Contains(minificationFileExtension));
        var resourcesWithoutUrlDebugVersion = resources.Where(r => !r.Url.Contains(minificationFileExtension));

        Assert.Equal(resourcesWithUrlDebugVersion.Select(r => r.Url.Replace(minificationFileExtension, string.Empty)), resourcesWithUrlDebugVersion.Select(r => r.UrlDebug));
        Assert.True(resourcesWithoutUrlDebugVersion.All(r => string.IsNullOrEmpty(r.UrlDebug)));
    }
}
