using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
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

        if (options.Credentials != null)
        {
            if (String.IsNullOrWhiteSpace(options.Credentials.SecretKey))
            {
                yield return new ValidationResult(Constants.ValidationMessages.SecretKeyIsEmpty);
            }

            if (String.IsNullOrWhiteSpace(options.Credentials.AccessKeyId))
            {
                yield return new ValidationResult(Constants.ValidationMessages.AccessKeyIdIsEmpty);
            }

            if (String.IsNullOrWhiteSpace(options.Credentials.RegionEndpoint))
            {
                yield return new ValidationResult(Constants.ValidationMessages.RegionEndpointIsEmpty);
            }

        }
    }

    public static AwsStorageOptions BindConfiguration(this AwsStorageOptions options, IShellConfiguration shellConfiguration)
    {
        var section = shellConfiguration.GetSection("OrchardCore_Media_Amazon_S3");

        if (section == null)
        {
            return options;
        }

        options.BucketName = section.GetValue(nameof(options.BucketName), String.Empty);
        options.BasePath = section.GetValue(nameof(options.BasePath), String.Empty);
        options.CreateBucket = section.GetValue(nameof(options.CreateBucket), false);

        var credentials = section.GetSection("Credentials");
        if (credentials.Exists())
        {
            options.Credentials = new AwsStorageCredentials
            {
                RegionEndpoint =
                    credentials.GetValue(nameof(options.Credentials.RegionEndpoint), RegionEndpoint.USEast1.SystemName),
                SecretKey = credentials.GetValue(nameof(options.Credentials.SecretKey), String.Empty),
                AccessKeyId = credentials.GetValue(nameof(options.Credentials.AccessKeyId), String.Empty),
            };

        }
        else
        {
            // Attempt to load Credentials from Profile.
            var profileName = section.GetValue("ProfileName", String.Empty);
            if (!String.IsNullOrEmpty(profileName))
            {
                var chain = new CredentialProfileStoreChain();
                if (chain.TryGetProfile(profileName, out var basicProfile))
                {
                    var awsCredentials = basicProfile.GetAWSCredentials(chain)?.GetCredentials();
                    if (awsCredentials != null)
                    {
                        options.Credentials = new AwsStorageCredentials
                        {
                            RegionEndpoint = basicProfile.Region.SystemName ?? RegionEndpoint.USEast1.SystemName,
                            SecretKey = awsCredentials.SecretKey,
                            AccessKeyId = awsCredentials.AccessKey
                        };
                    }
                }
            }
        }

        return options;
    }
}
