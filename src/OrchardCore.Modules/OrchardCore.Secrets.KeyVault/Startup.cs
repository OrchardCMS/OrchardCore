using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Secrets.KeyVault.Models;
using OrchardCore.Secrets.KeyVault.Services;

namespace OrchardCore.Secrets.KeyVault
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<KeyVaultClientService>();
            services.AddSingleton<ISecretStore, KeyVaultSecretStore>();
            services.AddTransient<IConfigureOptions<SecretsKeyVaultOptions>, KeyVaultOptionsConfiguration>();
        }
    }
}
