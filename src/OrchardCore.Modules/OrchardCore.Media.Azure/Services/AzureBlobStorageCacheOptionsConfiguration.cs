using Microsoft.Extensions.Options;
using OrchardCore.Media.ImageSharp;

namespace OrchardCore.Media.Azure.Services;

// Configuration of the ImageSharp Azure Blob Storage cache. We just pass the settings from the
// Orchard Core configuration.
internal sealed class AzureBlobStorageCacheOptionsConfiguration : IConfigureOptions<MediaImageSharpAzureBlobCacheOptions>
{
    private readonly ImageSharpBlobImageCacheOptions _options;

    public AzureBlobStorageCacheOptionsConfiguration(IOptions<ImageSharpBlobImageCacheOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(MediaImageSharpAzureBlobCacheOptions options)
    {
        options.ConnectionString = _options.ConnectionString;
        options.ContainerName = _options.ContainerName;
        options.CacheFolder = _options.BasePath;
    }
}
