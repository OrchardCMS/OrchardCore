using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        /// <summary>
        /// The path in the wwwroot folder containing the Azure Media Blob asset cache, auto prefixed by tenant.
        /// </summary>
        public string AssetsCachePath { get; set; } = "ab-cache";
    }
}
