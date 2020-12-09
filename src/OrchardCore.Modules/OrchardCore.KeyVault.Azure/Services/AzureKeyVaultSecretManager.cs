using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace OrchardCore.Azure.KeyVault.Services
{
    public class AzureKeyVaultSecretManager : KeyVaultSecretManager
    {
        public override string GetKey(KeyVaultSecret secret) =>
            secret.Name.Replace("---", "_").Replace("--", ":");
    }
}
