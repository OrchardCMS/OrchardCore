using Microsoft.Extensions.Options;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Media.ImageSharp;

namespace OrchardCore.Media.AmazonS3.Services;

// Configuration of the ImageSharp Amazon S3 cache. We just pass the settings from the Orchard Core
// configuration.
internal sealed class AWSS3StorageCacheOptionsConfiguration : IConfigureOptions<MediaImageSharpAmazonS3CacheOptions>
{
    private readonly AwsImageSharpImageCacheOptions _options;

    public AWSS3StorageCacheOptionsConfiguration(IOptions<AwsImageSharpImageCacheOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(MediaImageSharpAmazonS3CacheOptions options)
    {
        var credentials = _options.AwsOptions.Credentials.GetCredentials();

        // Only Endpoint or Region is necessary.
        options.Endpoint = _options.AwsOptions.DefaultClientConfig.ServiceURL;
        options.Region = _options.AwsOptions.Region?.SystemName;
        options.BucketName = _options.BucketName;
        options.AccessKey = credentials.AccessKey;
        options.AccessSecret = credentials.SecretKey;
        options.CacheFolder = _options.BasePath;
    }
}
