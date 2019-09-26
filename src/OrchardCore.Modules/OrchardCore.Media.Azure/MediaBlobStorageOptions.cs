using System;
using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        [Obsolete("PublicHostName is obsolete. Use MediaOptions.CdnBaseUrl instead.")]
        public string PublicHostName { get; set; }
    }
}
