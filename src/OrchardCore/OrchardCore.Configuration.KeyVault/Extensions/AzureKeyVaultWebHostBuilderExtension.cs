using System;
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
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureAppConfiguration((context, configuration) =>
            {
                var builtConfig = configuration.Build();
                var keyVaultName = builtConfig["OrchardCore:OrchardCore_KeyVault_Azure:KeyVaultName"];

                TimeSpan? reloadInterval = null;
                double interval;
                if (Double.TryParse(builtConfig["OrchardCore:OrchardCore_KeyVault_Azure:ReloadInterval"], out interval))
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
                    new DefaultAzureCredential(includeInteractiveCredentials: true),
                    configOptions
                );
            });

            return builder;
        }
    }
}
