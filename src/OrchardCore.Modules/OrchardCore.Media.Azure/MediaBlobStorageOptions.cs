using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        /// <summary>
        /// The path in the wwwroot folder containing the Azure Media Blob asset cache.
        /// The tenants name will be appended to this folder path.
        /// </summary>
        public string AssetsCachePath { get; set; } = "ab-cache";
    }
}
