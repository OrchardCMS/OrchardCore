using Amazon.S3;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage.AmazonS3;

namespace OrchardCore.Media.AmazonS3.Services;

public class AwsS3MediaImageCacheTenantEvents : AwsTenantEventsBase
{
    public AwsS3MediaImageCacheTenantEvents(
        ShellSettings shellSettings,
        IAmazonS3 amazonS3Client,
        IOptions<AwsMediaImageCacheOptions> options,
        IStringLocalizer<AwsS3MediaImageCacheTenantEvents> localizer,
        ILogger<AwsS3MediaImageCacheTenantEvents> logger)
        : base(shellSettings, amazonS3Client, options.Value, localizer, logger)
    {
    }
}
