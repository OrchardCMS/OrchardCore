using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Secrets.Azure.Models;
using OrchardCore.Secrets.Azure.Services;

namespace OrchardCore.Secrets.Azure;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<KeyVaultClientService>();
        services.AddSingleton<ISecretStore, KeyVaultSecretStore>();
        services.AddTransient<IConfigureOptions<SecretsKeyVaultOptions>, KeyVaultOptionsConfiguration>();
    }
}
