using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Azure.Services
{
    public class BlobTenantFileProvider : BlobFileProvider, ITenantFileProvider
    {
        public BlobTenantFileProvider(BlobStorageOptions options) : base(options)
        {
        }
    }
}
