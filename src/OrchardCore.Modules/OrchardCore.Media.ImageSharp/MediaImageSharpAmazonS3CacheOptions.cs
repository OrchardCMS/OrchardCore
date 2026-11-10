namespace OrchardCore.Media.ImageSharp;

/// <summary>
/// The settings of the Amazon S3 cache of the resized images, added with
/// <see cref="MediaImageSharpServiceCollectionExtensions.AddMediaImageSharpAmazonS3Cache"/>.
/// </summary>
public sealed class MediaImageSharpAmazonS3CacheOptions
{
    /// <summary>
    /// Gets or sets the service endpoint. Only the endpoint or the region is necessary.
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the system name of the region. Only the endpoint or the region is necessary.
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    /// Gets or sets the name of the bucket storing the resized images.
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the access key.
    /// </summary>
    public string AccessKey { get; set; }

    /// <summary>
    /// Gets or sets the access secret.
    /// </summary>
    public string AccessSecret { get; set; }

    /// <summary>
    /// Gets or sets the folder of the bucket storing the resized images.
    /// </summary>
    public string CacheFolder { get; set; }
}
