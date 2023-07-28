using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AmazonS3;

namespace OrchardCore.Media.AmazonS3;

public static class AwsStorageOptionsExtension
{
    public static IEnumerable<ValidationResult> Validate(this AwsStorageOptions options)
    {
        if (String.IsNullOrWhiteSpace(options.BucketName))
        {
            yield return new ValidationResult(Constants.ValidationMessages.BucketNameIsEmpty);
        }

        if (options.AwsOptions is not null)
        {
            if (options.AwsOptions.Region is null)
            {
                yield return new ValidationResult(Constants.ValidationMessages.RegionEndpointIsEmpty);
            }
        }
    }

    public static AwsStorageOptions BindConfiguration(this AwsStorageOptions options, IShellConfiguration shellConfiguration, ILogger logger)
    {
        var section = shellConfiguration.GetSection("OrchardCore_Media_AmazonS3");

        if (!section.Exists())
        {
            return options;
        }

        options.BucketName = section.GetValue(nameof(options.BucketName), String.Empty);
        options.BasePath = section.GetValue(nameof(options.BasePath), String.Empty);
        options.CreateBucket = section.GetValue(nameof(options.CreateBucket), false);
        options.RemoveBucket = section.GetValue(nameof(options.RemoveBucket), false);

        try
        {
            // Binding AWS Options.
            options.AwsOptions = shellConfiguration.GetAWSOptions("OrchardCore_Media_AmazonS3");

            // In case Credentials sections was specified, trying to add BasicAWSCredential to AWSOptions
            // since by design GetAWSOptions skips Credential section while parsing config.
            var credentials = section.GetSection("Credentials");
            if (credentials.Exists())
            {
                var secretKey = credentials.GetValue(Constants.AwsCredentialParamNames.SecretKey, String.Empty);
                var accessKey = credentials.GetValue(Constants.AwsCredentialParamNames.AccessKey, String.Empty);

                if (!String.IsNullOrWhiteSpace(accessKey) ||
                    !String.IsNullOrWhiteSpace(secretKey))
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
