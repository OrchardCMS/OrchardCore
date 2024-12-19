using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching.Azure;

namespace OrchardCore.Media.Azure.Services;

// Configuration for ImageSharp's own configuration object. We just pass the settings from the Orchard Core
// configuration.
internal sealed class AzureBlobStorageCacheOptionsConfiguration : IConfigureOptions<AzureBlobStorageCacheOptions>
{
    private readonly ImageSharpBlobImageCacheOptions _options;

    public AzureBlobStorageCacheOptionsConfiguration(IOptions<ImageSharpBlobImageCacheOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(AzureBlobStorageCacheOptions options)
    {
        options.ConnectionString = _options.ConnectionString;
        options.ContainerName = _options.ContainerName;
        options.CacheFolder = _options.BasePath;
    }
}
