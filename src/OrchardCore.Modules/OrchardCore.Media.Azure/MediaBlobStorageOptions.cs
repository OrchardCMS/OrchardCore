using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        /// <summary>
        /// Create blob container on startup if it does not exist.
        /// </summary>
        public bool CreateContainer { get; set; }

        /// <summary>
        /// Remove blob container on tenant removal if it exists.
        /// </summary>
        public bool RemoveContainer { get; set; }
    }
}
