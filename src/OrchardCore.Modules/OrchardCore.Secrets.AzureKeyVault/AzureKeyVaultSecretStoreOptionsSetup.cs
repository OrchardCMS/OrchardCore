using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Secrets.AzureKeyVault;

public class AzureKeyVaultSecretStoreOptionsSetup : IConfigureOptions<AzureKeyVaultSecretStoreOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public AzureKeyVaultSecretStoreOptionsSetup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(AzureKeyVaultSecretStoreOptions options)
    {
        var section = _shellConfiguration.GetSection("Secrets:AzureKeyVault");

        options.AzureClient = section["AzureClient"] ?? options.AzureClient;
        options.NamePrefix = section["NamePrefix"] ?? options.NamePrefix;
    }
}
