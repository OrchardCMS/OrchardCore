using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Secrets.Azure;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AzureKeyVaultSecretStoreOptions>, AzureKeyVaultSecretStoreOptionsSetup>();
        services.AddSecretStore<AzureKeyVaultSecretStore>();
    }
}
