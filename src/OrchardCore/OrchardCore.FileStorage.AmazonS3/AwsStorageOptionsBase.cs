using Amazon.Extensions.NETCore.Setup;

namespace OrchardCore.FileStorage.AmazonS3;

public abstract class AwsStorageOptionsBase : IAwsStorageOptions
{
    /// <inheritdoc/>
    public string BucketName { get; set; }

    /// <inheritdoc/>

    public string BasePath { get; set; }

    /// <inheritdoc/>

    public bool CreateBucket { get; set; }

    /// <inheritdoc/>

    public AWSOptions AwsOptions { get; set; }

    /// <inheritdoc/>

    public bool RemoveBucket { get; set; }
}
