using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        public string PublicHostName { get; set; }
        public bool SupportResizing { get; set; }

        /// <summary>
        /// Time to expire the Blob ContentMD5 File Version Hash from cache, in minutes, defaults to 2 hours
        /// </summary>
        public int VersionHashCacheExpiryTime { get; set; } = 120;
    }
}
