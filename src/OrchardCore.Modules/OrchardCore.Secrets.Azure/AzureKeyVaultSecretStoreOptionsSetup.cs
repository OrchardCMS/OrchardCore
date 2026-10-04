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
        var section = _shellConfiguration.GetSection("Secrets:Azure");

        options.AzureClient = section["AzureClient"] ?? options.AzureClient;
        options.NamePrefix = section["NamePrefix"] ?? options.NamePrefix;
    }
}
