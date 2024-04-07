using Amazon.S3;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage.AmazonS3;

namespace OrchardCore.Media.AmazonS3.Services;

public class ImageSharpS3ImageCacheBucketTenantEvents : AwsTenantEventsBase
{
    public ImageSharpS3ImageCacheBucketTenantEvents(
        ShellSettings shellSettings,
        IAmazonS3 amazonS3Client,
        IOptions<AwsImageSharpImageCacheOptions> options,
        IStringLocalizer<ImageSharpS3ImageCacheBucketTenantEvents> localizer,
        ILogger<ImageSharpS3ImageCacheBucketTenantEvents> logger)
        : base(shellSettings, amazonS3Client, options.Value, localizer, logger)
    {
    }
}
