namespace OrchardCore.FileStorage.AmazonS3;

/// <summary>
/// AWS storage options.
/// </summary>
public class AwsStorageOptions
{
    /// <summary>
    /// AWS S3 bucket name.
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// The base directory path to use inside the container for this store's contents.
    /// </summary>
    public string BasePath { get; set; }

    /// <summary>
    /// Gets or sets the credentials.
    /// <remarks>
    /// Credentials section can be set directly via configuration or get loaded from the configured ProfileName.
    /// For development purpose it is recommended to specify just ProfileName.
    /// For Prod Env this section should be null, AWS SDK Services will get the default credentials from Env variables
    /// </remarks>
    /// </summary>
    public AwsStorageCredentials Credentials { get; set; }

}

/// <summary>
/// The AWS storage credentials.
/// </summary>
public class AwsStorageCredentials
{
    /// <summary>
    /// AWS region name
    /// </summary>
    public string RegionEndpoint { get; set; }

    /// <summary>
    /// AWS account secret key. Do not use root's user secret key!
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// AWS account access key Id.
    /// </summary>
    public string AccessKeyId { get; set; }
}
