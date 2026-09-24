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

    // The 'OrchardCore_Media_Azure_ImageSharp_Cache' section is deprecated and will be removed in a future major version, use 'Media:Azure:ImageCache' instead.
    protected override MediaBlobImageCacheOptions GetRawOptions()
        => _shellConfiguration.GetSectionCompat("Media:Azure:ImageCache", "OrchardCore_Media_Azure_ImageSharp_Cache")
        .Get<MediaBlobImageCacheOptions>();
}
