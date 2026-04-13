using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using OrchardCore.Modules.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class SlugifyMediaNameNormalizerServiceTests
{
    private readonly SlugService _slugService = new();

    [Fact]
    public void ShouldNormalizeFolderNameWithTransliterationEnabledByDefault()
    {
        var options = new MediaSlugifyOptions();
        var service = new SlugifyMediaNameNormalizerService(_slugService, Options.Create(options));

        var normalizedFolderName = service.NormalizeFolderName("Æther");

        Assert.Equal("aether", normalizedFolderName);
    }

    [Fact]
    public void ShouldNormalizeFileNameWithoutTransliterationWhenDisabled()
    {
        var options = new MediaSlugifyOptions
        {
            Transliterate = false,
        };
        var service = new SlugifyMediaNameNormalizerService(_slugService, Options.Create(options));

        var normalizedFileName = service.NormalizeFileName("Æther.png");

        Assert.Equal("æther.png", normalizedFileName);
    }

    [Fact]
    public void ShouldDefaultTransliterationToTrueWhenConfigurationIsMissing()
    {
        var options = new MediaSlugifyOptions
        {
            Transliterate = false,
        };

        var shellConfiguration = CreateShellConfiguration([]);
        var configuration = new MediaSlugifyOptionsConfiguration(shellConfiguration);

        configuration.Configure(options);

        Assert.True(options.Transliterate);
    }

    [Fact]
    public void ShouldReadTransliterationFromConfiguration()
    {
        var shellConfiguration = CreateShellConfiguration(
        [
            new KeyValuePair<string, string>("OrchardCore:OrchardCore_Media_Slugify:Transliterate", "false"),
        ]);
        var options = new MediaSlugifyOptions();
        var configuration = new MediaSlugifyOptionsConfiguration(shellConfiguration);

        configuration.Configure(options);

        Assert.False(options.Transliterate);
    }

    private static ShellConfiguration CreateShellConfiguration(IEnumerable<KeyValuePair<string, string>> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new ShellConfiguration(configuration.GetSection("OrchardCore"));
    }
}
