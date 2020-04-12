using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        /// <summary>
        /// Create blob container on startup if it does not exist.
        /// </summary>
        public bool CreateContainer { get; set; }
    }
}
