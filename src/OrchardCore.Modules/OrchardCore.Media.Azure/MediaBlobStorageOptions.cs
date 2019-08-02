using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        //TODO Probably add Blob Security Level
        public string PublicHostName { get; set; }

        /// <summary>
        /// Time to expire the Blob ContentMD5 File Version Hash from cache, in minutes, defaults to 2 hours
        /// </summary>
        public int VersionHashCacheExpiryTime { get; set; } = 120;
    }
}
