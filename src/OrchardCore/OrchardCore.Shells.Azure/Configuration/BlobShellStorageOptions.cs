using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Shells.Azure.Configuration
{
    public class BlobShellStorageOptions : BlobStorageOptions
    {
        public bool MigrateFromFiles { get; set; }
    }
}
