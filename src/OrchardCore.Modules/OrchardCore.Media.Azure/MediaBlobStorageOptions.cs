using System;
using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        [Obsolete("PublicHostName is obsolete. Use MediaOptions.CdnBaseUrl instead.")]
        public string PublicHostName { get; set; }

        /// <summary>
        /// Create blob container on startup if it does not exist.
        /// </summary>
        public bool CreateContainer { get; set; }
       
    }
}
