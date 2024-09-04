using System.ComponentModel.DataAnnotations;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AmazonS3;

namespace OrchardCore.Media.AmazonS3;

public static class AwsStorageOptionsExtension
{
    public static IEnumerable<ValidationResult> Validate(this AwsStorageOptionsBase options)
    {
        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            yield return new ValidationResult(AmazonS3Constants.ValidationMessages.BucketNameIsEmpty);
        }

        if (options.AwsOptions is not null)
        {
            if (options.AwsOptions.Region is null && options.AwsOptions.DefaultClientConfig.ServiceURL is null)
            {
                yield return new ValidationResult(AmazonS3Constants.ValidationMessages.RegionAndServiceUrlAreEmpty);
            }
        }
    }

    public static AwsStorageOptionsBase BindConfiguration(this AwsStorageOptionsBase options, string configSection, IShellConfiguration shellConfiguration, ILogger logger)
    {
        var section = shellConfiguration.GetSection(configSection);

        if (!section.Exists())
        {
            return options;
        }

        options.BucketName = section.GetValue(nameof(options.BucketName), string.Empty);
        options.BasePath = section.GetValue(nameof(options.BasePath), string.Empty);
        options.CreateBucket = section.GetValue(nameof(options.CreateBucket), false);
        options.RemoveBucket = section.GetValue(nameof(options.RemoveBucket), false);

        try
        {
            // Binding AWS Options. Using the AmazonS3Config type parameter is necessary to be able to configure
            // S3-specific properties like ForcePathStyle via the configuration provider.
            options.AwsOptions = shellConfiguration.GetAWSOptions<AmazonS3Config>("OrchardCore_Media_AmazonS3");

            // In case Credentials sections was specified, trying to add BasicAWSCredential to AWSOptions
            // since by design GetAWSOptions skips Credential section while parsing config.
            var credentials = section.GetSection("Credentials");
            if (credentials.Exists())
            {
                var secretKey = credentials.GetValue(AmazonS3Constants.AwsCredentialParamNames.SecretKey, string.Empty);
                var accessKey = credentials.GetValue(AmazonS3Constants.AwsCredentialParamNames.AccessKey, string.Empty);

                if (!string.IsNullOrWhiteSpace(accessKey) ||
                    !string.IsNullOrWhiteSpace(secretKey))
                {
                    var awsCredentials = new BasicAWSCredentials(accessKey, secretKey);
                    options.AwsOptions.Credentials = awsCredentials;
                }
            }

            return options;
        }
        catch (ConfigurationException ex)
        {
            logger.LogCritical(ex, "Failed to configure AWS options.");
            throw;
        }
    }
}
