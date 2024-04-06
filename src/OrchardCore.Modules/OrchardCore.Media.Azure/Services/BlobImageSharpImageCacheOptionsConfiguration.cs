using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media.Azure.Helpers;

namespace OrchardCore.Media.Azure.Services;

internal class BlobImageSharpImageCacheOptionsConfiguration : IConfigureOptions<ImageSharpBlobImageCacheOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public BlobImageSharpImageCacheOptionsConfiguration(
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<BlobImageSharpImageCacheOptionsConfiguration> logger)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(ImageSharpBlobImageCacheOptions options)
    {
        var section = _shellConfiguration.GetSection("OrchardCore_Media_Azure_ImageSharp_Cache");
        section.Bind(options);

        var fluidParserHelper = new FluidParserHelper<ImageSharpBlobImageCacheOptions>(_shellSettings);

        try
        {
            // Container name must be lowercase.
            options.ContainerName = fluidParserHelper.ParseAndFormat(options.ContainerName).ToLowerInvariant();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unable to parse Azure Media ImageSharp Image Cache container name.");
            throw;
        }

        try
        {
            options.BasePath = fluidParserHelper.ParseAndFormat(options.BasePath);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Azure Media ImageSharp Image Cache base path.");
            throw;
        }
    }
}
