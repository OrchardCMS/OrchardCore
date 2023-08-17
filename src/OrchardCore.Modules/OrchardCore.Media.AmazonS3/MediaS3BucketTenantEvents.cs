using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Modules;

namespace OrchardCore.Media.AmazonS3;

public class MediaS3BucketTenantEvents : ModularTenantEvents
{
    private readonly ShellSettings _shellSettings;
    private readonly AwsStorageOptions _options;
    private readonly IAmazonS3 _amazonS3Client;
    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public MediaS3BucketTenantEvents(
        ShellSettings shellSettings,
        IAmazonS3 amazonS3Client,
        IOptions<AwsStorageOptions> options,
        IStringLocalizer<MediaS3BucketTenantEvents> localizer,
        ILogger<MediaS3BucketTenantEvents> logger)
    {
        _shellSettings = shellSettings;
        _amazonS3Client = amazonS3Client;
        _options = options.Value;
        S = localizer;
        _logger = logger;
    }

    public override async Task ActivatingAsync()
    {
        if (!_options.CreateBucket ||
            _shellSettings.IsUninitialized() ||
            String.IsNullOrEmpty(_options.BucketName))
        {
            return;
        }

        _logger.LogDebug("Testing Amazon S3 Bucket {BucketName} existence", _options.BucketName);

        try
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3Client, _options.BucketName);
            if (bucketExists)
            {
                _logger.LogInformation("Amazon S3 Bucket {BucketName} already exists.", _options.BucketName);

                return;
            }

            var bucketRequest = new PutBucketRequest
            {
                BucketName = _options.BucketName,
                UseClientRegion = true
            };

            // Trying to create bucket.
            var response = await _amazonS3Client.PutBucketAsync(bucketRequest);

            if (!response.IsSuccessful())
            {
                _logger.LogError("Unable to create Amazon S3 Bucket {BucketName}", _options.BucketName);

                return;
            }

            // Blocking public access for the newly created bucket.
            var blockConfiguration = new PublicAccessBlockConfiguration
            {
                BlockPublicAcls = true,
                BlockPublicPolicy = true,
                IgnorePublicAcls = true,
                RestrictPublicBuckets = true
            };

            await _amazonS3Client.PutPublicAccessBlockAsync(new PutPublicAccessBlockRequest
            {
                PublicAccessBlockConfiguration = blockConfiguration,
                BucketName = _options.BucketName
            });

            _logger.LogDebug("Amazon S3 Bucket {BucketName} created.", _options.BucketName);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Unable to create Amazon S3 Bucket.");
        }
    }

    public override async Task RemovingAsync(ShellRemovingContext context)
    {
        if (!_options.RemoveBucket || String.IsNullOrEmpty(_options.BucketName))
        {
            return;
        }

        try
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3Client, _options.BucketName);
            if (!bucketExists)
            {
                return;
            }

            var bucketRequest = new DeleteBucketRequest
            {
                BucketName = _options.BucketName,
                UseClientRegion = true
            };

            // Trying to delete bucket.
            var response = await _amazonS3Client.DeleteBucketAsync(bucketRequest);
            if (!response.IsSuccessful())
            {
                _logger.LogError("Failed to remove the Amazon S3 Bucket {BucketName}", _options.BucketName);
                context.ErrorMessage = S["Failed to remove the Amazon S3 Bucket '{0}'.", _options.BucketName];
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to remove the Amazon S3 Bucket {BucketName}", _options.BucketName);
            context.ErrorMessage = S["Failed to remove the Amazon S3 Bucket '{0}'.", _options.BucketName];
            context.Error = ex;
        }
    }
}
