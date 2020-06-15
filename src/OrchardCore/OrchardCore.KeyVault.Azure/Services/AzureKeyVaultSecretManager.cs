using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace OrchardCore.Azure.KeyVault.Services
{
    public class AzureKeyVaultSecretManager : DefaultKeyVaultSecretManager
    {
        public override string GetKey(SecretBundle secret) =>
            secret.SecretIdentifier.Name.Replace("---", "_").Replace("--", ":");
    }
}
