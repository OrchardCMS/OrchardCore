using System.ComponentModel.DataAnnotations;
using Amazon.Runtime;
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
        else if (!string.IsNullOrWhiteSpace(options.BucketName))
        {
            yield return new ValidationResult(AmazonS3Constants.ValidationMessages.AwsOptionsAreInvalid);
        }
    }

    public static AwsStorageOptionsBase BindConfiguration(this AwsStorageOptionsBase options, string configSection, IShellConfiguration shellConfiguration, ILogger logger)
        => options.BindConfiguration(shellConfiguration.GetSection(configSection), shellConfiguration, logger);

    /// <summary>
    /// Binds the options from a configuration section that is still supported under a legacy name, the values of the
    /// section winning over the same values of the legacy section.
    /// </summary>
    public static AwsStorageOptionsBase BindConfiguration(this AwsStorageOptionsBase options, string configSection, string legacyConfigSection, IShellConfiguration shellConfiguration, ILogger logger)
        => options.BindConfiguration(shellConfiguration.GetSectionCompat(configSection, legacyConfigSection), shellConfiguration, logger);

    private static AwsStorageOptionsBase BindConfiguration(this AwsStorageOptionsBase options, IConfigurationSection section, IShellConfiguration shellConfiguration, ILogger logger)
    {

        if (!section.Exists())
        {
            return options;
        }

        try
        {
            options.BucketName = section.GetValue(nameof(options.BucketName), string.Empty);
            options.BasePath = section.GetValue(nameof(options.BasePath), string.Empty);
            options.CreateBucket = section.GetValue(nameof(options.CreateBucket), false);
            options.RemoveBucket = section.GetValue(nameof(options.RemoveBucket), false);

            // Binding AWS Options. Using the AmazonS3Config type parameter is necessary to be able to configure
            // S3-specific properties like ForcePathStyle via the configuration provider.
            var awsSection = shellConfiguration.GetSectionCompat(AmazonS3Constants.ConfigSections.AmazonS3, AmazonS3Constants.ConfigSections.LegacyAmazonS3);
            options.AwsOptions = awsSection.GetAWSOptions(string.Empty);

            // In case Credentials sections was specified, trying to add BasicAWSCredential to AWSOptions
            // since by design GetAWSOptions skips Credential section while parsing config.
            var credentials = section.GetSection("Credentials");
            if (!credentials.Exists())
            {
                credentials = awsSection.GetSection("Credentials");
            }

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
        }
        catch (Exception ex)
        {
            // A malformed value shouldn't prevent the tenant from starting: without AWS options the validation fails
            // and the features using these options keep the local file storage.
            logger.LogCritical(ex, "Failed to configure AWS options.");

            options.AwsOptions = null;
        }

        return options;
    }
}
