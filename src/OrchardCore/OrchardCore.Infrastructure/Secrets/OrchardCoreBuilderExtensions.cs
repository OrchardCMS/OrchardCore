using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Options;
using OrchardCore.Secrets.Services;
using OrchardCore.Secrets.Stores;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services to manage secrets.
        /// </summary>
        public static OrchardCoreBuilder AddSecrets(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddSingleton<ISecretService, SecretService>();
                services.Configure<SecretOptions>(options =>
                {
                    options.SecretTypes.Add(typeof(TextSecret));
                    options.SecretTypes.Add(typeof(RSASecret));
                });

                services.AddSingleton<SecretInfosManager>();
                services.AddSingleton<SecretsDocumentManager>();

                services.AddSingleton<ISecretStore, DatabaseSecretStore>();
                services.AddSingleton<ISecretStore, ConfigurationSecretStore>();

                services.AddSingleton<ISecretProtectionProvider, SecretProtectionProvider>();
                services.AddSingleton<ISecretTokenService, SecretTokenService>();
            });
        }
    }
}
