using Amazon.Extensions.NETCore.Setup;

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
    /// Indicates if bucket should be created on module startup.
    /// </summary>
    public bool CreateBucket { get; set; }

    /// <summary>
    /// Gets or sets the AWS Options.
    /// </summary>
    public AWSOptions AwsOptions { get; set; }

    /// <summary>
    /// Indicates if bucket should be removed on tenant removal.
    /// </summary>
    public bool RemoveBucket { get; set; }
}
