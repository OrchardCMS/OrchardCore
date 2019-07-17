using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptions : BlobStorageOptions
    {
        public string PublicHostName { get; set; }
        public bool SupportResizing { get; set; }
    }
}
