using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Modules;

namespace OrchardCore.Media.AmazonS3;

public class CreateMediaS3BucketEvent : ModularTenantEvents
{
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;
    private readonly AwsStorageOptions _options;
    private readonly IAmazonS3 _amazonS3Client;

    public CreateMediaS3BucketEvent(ShellSettings shellSettings,
        IOptions<AwsStorageOptions> options,
        IAmazonS3 amazonS3Client,
        ILogger<CreateMediaS3BucketEvent> logger)
    {
        _shellSettings = shellSettings;
        _logger = logger;
        _amazonS3Client = amazonS3Client;
        _options = options.Value;
    }

    public override async Task ActivatingAsync()
    {
        if (_options.CreateBucket &&
            _shellSettings.State != Environment.Shell.Models.TenantState.Uninitialized &&
            !String.IsNullOrEmpty(_options.BucketName))
        {
            _logger.LogDebug("Testing Amazon S3 Bucket {BucketName} existence", _options.BucketName);

            try
            {
                var isBucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3Client, _options.BucketName);
                if (isBucketExists)
                {
                    _logger.LogInformation("Amazon S3 Bucket {BucketName} already exists.", _options.BucketName);
                    return;
                }

                var bucketRequest = new PutBucketRequest
                {
                    BucketName = _options.BucketName,
                    UseClientRegion = true
                };

                // Tying to create bucket
                var response = await _amazonS3Client.PutBucketAsync(bucketRequest);

                if (!response.IsSuccessful())
                {
                    _logger.LogError("Unable to create Amazon S3 Bucket. {Response}", response);
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
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to create Amazon S3 Bucket.");
            }
        }
    }
}
