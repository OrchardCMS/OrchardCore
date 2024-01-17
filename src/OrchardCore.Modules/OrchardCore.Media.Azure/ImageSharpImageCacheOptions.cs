namespace OrchardCore.Media.Azure;

public class ImageSharpImageCacheOptions
{
    /// <summary>
    /// The Azure Blob connection string.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// The Azure Blob container name.
    /// </summary>
    public string ContainerName { get; set; }

    /// <summary>
    /// Create blob container on startup if it does not exist.
    /// </summary>
    public bool CreateContainer { get; set; } = true;

    /// <summary>
    /// Remove blob container on tenant removal if it exists.
    /// </summary>
    public bool RemoveContainer { get; set; }
}
