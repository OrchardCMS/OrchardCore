using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrchardCore.Azure.KeyVault.Services;

namespace OrchardCore.Azure.KeyVault.Extensions
{
    public static class AzureKeyVaultWebHostBuilderExtension
    {
        /// <summary>
        /// Adds Azure Key Vault as a Configuration Source.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <returns>The web host builder.</returns>
        public static IHostBuilder UseOrchardCoreAzureKeyVault(this IHostBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureAppConfiguration((context, configuration) =>
            {
                var builtConfig = configuration.Build();
                var keyVaultName = builtConfig["OrchardCore:OrchardCore_Azure_KeyVault:KeyVaultName"];
                var clientId = builtConfig["OrchardCore:OrchardCore_Azure_KeyVault:AzureADApplicationId"];
                var clientSecret = builtConfig["OrchardCore:OrchardCore_Azure_KeyVault:AzureADApplicationSecret"];

                var keyVaultEndpoint = "https://" + keyVaultName + ".vault.azure.net";
                configuration.AddAzureKeyVault(
                    keyVaultEndpoint, 
                    clientId, 
                    clientSecret,  
                    new CustomKeyVaultSecretManager()
                );
            });

            return builder;
        }


    }
}
