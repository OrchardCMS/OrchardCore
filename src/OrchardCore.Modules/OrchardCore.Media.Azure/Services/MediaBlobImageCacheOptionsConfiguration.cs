using Fluid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure.Services;

internal sealed class MediaBlobImageCacheOptionsConfiguration : BlobStorageOptionsConfiguration<MediaBlobImageCacheOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public MediaBlobImageCacheOptionsConfiguration(
        FluidParser fluidParser,
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<MediaBlobImageCacheOptionsConfiguration> logger)
         : base(fluidParser, shellSettings, logger)
    {
        _shellConfiguration = shellConfiguration;
    }

    protected override void FurtherConfigure(MediaBlobImageCacheOptions rawOptions, MediaBlobImageCacheOptions options)
    {
        options.CreateContainer = rawOptions.CreateContainer;
        options.RemoveContainer = rawOptions.RemoveContainer;
    }

    protected override MediaBlobImageCacheOptions GetRawOptions()
        => _shellConfiguration.GetSection("OrchardCore_Media_Azure_Image_Cache")
        .Get<MediaBlobImageCacheOptions>();
}
