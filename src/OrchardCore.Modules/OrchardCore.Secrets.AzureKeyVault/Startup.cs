using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Secrets.AzureKeyVault;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AzureKeyVaultSecretStoreOptions>, AzureKeyVaultSecretStoreOptionsSetup>();
        services.AddOptions<AzureKeyVaultSecretStoreOptions>()
            .Validate(options => !string.IsNullOrWhiteSpace(options.AzureClient),
                "'OrchardCore:Secrets:AzureKeyVault:AzureClient' must name a host-registered keyed SecretClient.")
            .Validate(options => options.AzureClient != AzureKeyVaultSecretStore.SecretClientServiceKey,
                "'OrchardCore:Secrets:AzureKeyVault:AzureClient' must not reference the feature's own service key.");
        services.AddKeyedSingleton<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey,
            (provider, _) =>
            {
                var name = provider.GetRequiredService<IOptions<AzureKeyVaultSecretStoreOptions>>().Value.AzureClient;
                return provider.GetKeyedService<SecretClient>(name)
                    ?? throw new InvalidOperationException($"No host-registered SecretClient has the service key '{name}'.");
            });
        services.AddSecretStore<AzureKeyVaultSecretStore>();
    }
}
