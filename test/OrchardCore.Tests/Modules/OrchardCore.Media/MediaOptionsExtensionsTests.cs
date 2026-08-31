using Microsoft.Extensions.Options;
using OrchardCore.Media;
using OrchardCore.Media.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaOptionsExtensionsTests
{
    [Theory]
    [InlineData(".jpg")]
    [InlineData(".JPG")]
    public void IsFileExtensionAllowed_StandardExtension_AllowsWithoutAdditionalPermission(string extension)
    {
        Assert.True(CreateOptions().IsFileExtensionAllowed(extension, hasAdditionalPermission: false));
    }

    [Fact]
    public void IsFileExtensionAllowed_RestrictedExtension_DeniesWithoutAdditionalPermission()
    {
        Assert.False(CreateOptions().IsFileExtensionAllowed(".svg", hasAdditionalPermission: false));
    }

    [Theory]
    [InlineData(".svg")]
    [InlineData(".SVG")]
    public void IsFileExtensionAllowed_RestrictedExtension_AllowsWithAdditionalPermission(string extension)
    {
        Assert.True(CreateOptions().IsFileExtensionAllowed(extension, hasAdditionalPermission: true));
    }

    [Fact]
    public void IsFileExtensionAllowed_UnconfiguredExtension_DeniesWithAdditionalPermission()
    {
        Assert.False(CreateOptions().IsFileExtensionAllowed(".exe", hasAdditionalPermission: true));
    }

    [Fact]
    public void Validate_OverlappingExtensions_Fails()
    {
        var options = CreateOptions();
        options.AllowedFileExtensions.Add(".SVG");

        var result = new MediaOptionsValidator().Validate(Options.DefaultName, options);

        Assert.True(result.Failed);
        Assert.Contains(".svg", result.FailureMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static MediaOptions CreateOptions() => new()
    {
        AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg" },
        RestrictedFileExtensions = new(StringComparer.OrdinalIgnoreCase) { ".svg" },
    };
}
