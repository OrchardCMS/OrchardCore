using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        private const int DefaultVersionHashCacheExpiryTime = 120;

        /// <summary>
        /// Time to expire the Blob ContentMD5 File Version Hash from the memory cache, in minutes, defaults to 2 hours.
        /// </summary>
        public int VersionHashCacheExpiryTime { get; set; } = DefaultVersionHashCacheExpiryTime;
    }
}
