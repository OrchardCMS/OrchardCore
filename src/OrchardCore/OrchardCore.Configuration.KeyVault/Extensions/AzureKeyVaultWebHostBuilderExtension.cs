using System;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrchardCore.Configuration.KeyVault.Services;

namespace OrchardCore.Configuration.KeyVault.Extensions
{
    public static class AzureKeyVaultWebHostBuilderExtension
    {
        /// <summary>
        /// Adds Azure Key Vault as a Configuration Source.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <returns>The web host builder.</returns>
        public static IHostBuilder AddOrchardCoreAzureKeyVault(this IHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddOrchardCoreAzureKeyVault(new DefaultAzureCredential(includeInteractiveCredentials: true));

            return builder;
        }

        /// <summary>
        /// Adds Azure Key Vault as a Configuration Source.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="tokenCredential">The token credential to use for authentication.</param>
        /// <returns>The web host builder.</returns>
        public static IHostBuilder AddOrchardCoreAzureKeyVault(this IHostBuilder builder, TokenCredential tokenCredential)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureAppConfiguration((context, configuration) =>
            {
                var builtConfig = configuration.Build();
                var keyVaultName = builtConfig["OrchardCore:OrchardCore_KeyVault_Azure:KeyVaultName"];

                TimeSpan? reloadInterval = null;
                if (Double.TryParse(builtConfig["OrchardCore:OrchardCore_KeyVault_Azure:ReloadInterval"], out var interval))
                {
                    reloadInterval = TimeSpan.FromSeconds(interval);
                }

                var keyVaultEndpointUri = new Uri("https://" + keyVaultName + ".vault.azure.net");
                var configOptions = new AzureKeyVaultConfigurationOptions()
                {
                    Manager = new AzureKeyVaultSecretManager(),
                    ReloadInterval = reloadInterval
                };

                configuration.AddAzureKeyVault(
                    keyVaultEndpointUri,
                    tokenCredential,
                    configOptions
                );
            });

            return builder;
        }
    }
}
