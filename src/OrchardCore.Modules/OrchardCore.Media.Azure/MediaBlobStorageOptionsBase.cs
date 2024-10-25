using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure;

public abstract class MediaBlobStorageOptionsBase : BlobStorageOptions
{
    /// <summary>
    /// Create a Blob container on startup if one does not exist.
    /// </summary>
    public bool CreateContainer { get; set; } = true;

    /// <summary>
    /// Remove Blob container on tenant removal if it exists.
    /// </summary>
    public bool RemoveContainer { get; set; }
}
