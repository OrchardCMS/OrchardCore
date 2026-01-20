using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure.Services;

internal sealed class ImageSharpBlobImageCacheOptionsConfiguration : BlobStorageOptionsConfiguration<ImageSharpBlobImageCacheOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public ImageSharpBlobImageCacheOptionsConfiguration(
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<ImageSharpBlobImageCacheOptionsConfiguration> logger)
         : base(shellSettings, logger)
    {
        _shellConfiguration = shellConfiguration;
    }

    protected override void FurtherConfigure(ImageSharpBlobImageCacheOptions rawOptions, ImageSharpBlobImageCacheOptions options)
    {
        options.CreateContainer = rawOptions.CreateContainer;
        options.RemoveContainer = rawOptions.RemoveContainer;
    }

    protected override ImageSharpBlobImageCacheOptions GetRawOptions()
        => _shellConfiguration.GetSection("OrchardCore_Media_Azure_ImageSharp_Cache")
        .Get<ImageSharpBlobImageCacheOptions>();
}
