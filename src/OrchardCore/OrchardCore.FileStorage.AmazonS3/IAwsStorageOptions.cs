using Amazon.Extensions.NETCore.Setup;

namespace OrchardCore.FileStorage.AmazonS3;

/// <summary>
/// Contract for all types providing options to AWS storage.
/// </summary>
public interface IAwsStorageOptions
{
    /// <summary>
    /// AWS S3 bucket name.
    /// </summary>
    string BucketName { get; set; }

    /// <summary>
    /// The base directory path to use inside the container for this store's contents.
    /// </summary>
    string BasePath { get; set; }

    /// <summary>
    /// Indicates if bucket should be created on module startup.
    /// </summary>
    bool CreateBucket { get; set; }

    /// <summary>
    /// Gets or sets the AWS Options.
    /// </summary>
    AWSOptions AwsOptions { get; set; }

    /// <summary>
    /// Indicates if bucket should be removed on tenant removal.
    /// </summary>
    bool RemoveBucket { get; set; }
}
