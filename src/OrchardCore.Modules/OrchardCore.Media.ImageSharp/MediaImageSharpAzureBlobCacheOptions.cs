namespace OrchardCore.Media.ImageSharp;

/// <summary>
/// The settings of the Azure Blob Storage cache of the resized images, added with
/// <see cref="MediaImageSharpServiceCollectionExtensions.AddMediaImageSharpAzureBlobCache"/>.
/// </summary>
public sealed class MediaImageSharpAzureBlobCacheOptions
{
    /// <summary>
    /// Gets or sets the Azure Blob Storage connection string.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the container storing the resized images.
    /// </summary>
    public string ContainerName { get; set; }

    /// <summary>
    /// Gets or sets the folder of the container storing the resized images.
    /// </summary>
    public string CacheFolder { get; set; }
}
