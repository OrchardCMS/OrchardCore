using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Secrets.Azure;

public class AzureKeyVaultSecretStoreOptionsSetup : IConfigureOptions<AzureKeyVaultSecretStoreOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public AzureKeyVaultSecretStoreOptionsSetup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(AzureKeyVaultSecretStoreOptions options)
    {
        var section = _shellConfiguration.GetSection("OrchardCore_Secrets_Azure");

        options.VaultUri = section["VaultUri"];
        options.TenantId = section["TenantId"];
        options.ClientId = section["ClientId"];
        options.ClientSecret = section["ClientSecret"];
    }
}
