using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching.Azure;

namespace OrchardCore.Media.Azure.Services;

// Configuration for ImageSharp's own configuration object. We just pass the settings from the Orchard Core
// configuration.
internal class AzureBlobStorageCacheOptionsConfiguration : IConfigureOptions<AzureBlobStorageCacheOptions>
{
    private readonly IOptions<ImageSharpImageCacheOptions> _options;

    public AzureBlobStorageCacheOptionsConfiguration(IOptions<ImageSharpImageCacheOptions> options)
    {
        _options = options;
    }

    public void Configure(AzureBlobStorageCacheOptions options)
    {
        options.ConnectionString = _options.Value.ConnectionString;
        options.ContainerName = _options.Value.ContainerName;
    }
}
