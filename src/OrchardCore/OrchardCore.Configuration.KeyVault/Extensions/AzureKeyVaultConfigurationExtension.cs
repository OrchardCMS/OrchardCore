using System;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrchardCore.Configuration.KeyVault.Services;

namespace OrchardCore.Configuration.KeyVault.Extensions
{
    public static class AzureKeyVaultConfigurationExtension
    {
        /// <summary>
        /// Adds Azure Key Vault as a Configuration Source.
        /// </summary>
        /// <param name="builder">The host builder to configure.</param>
        /// <returns>The host builder.</returns>
        public static IHostBuilder AddOrchardCoreAzureKeyVault(this IHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddOrchardCoreAzureKeyVault(new DefaultAzureCredential(includeInteractiveCredentials: true));
        }

        /// <summary>
        /// Adds Azure Key Vault as a Configuration Source.
        /// </summary>
        /// <param name="builder">The host builder to configure.</param>
        /// <param name="tokenCredential">The token credential to use for authentication.</param>
        /// <returns>The host builder.</returns>
        public static IHostBuilder AddOrchardCoreAzureKeyVault(this IHostBuilder builder, TokenCredential tokenCredential)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureAppConfiguration((context, builder) =>
            {
                // Here 'configuration' is a manager being a builder, and also an 'IConfigurationRoot'
                // if get from the 'context', allowing to get values from the providers already added
                // without having to build a configuration on the fly that would need to be disposed.
                AddOrchardCoreAzureKeyVault(builder, context.Configuration, tokenCredential);
            });

            return builder;
        }
        /// <summary>
        /// Adds Azure Key Vault as a Configuration Source.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <returns>The web host builder.</returns>
        public static IWebHostBuilder AddOrchardCoreAzureKeyVault(this IWebHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddOrchardCoreAzureKeyVault(new DefaultAzureCredential(includeInteractiveCredentials: true));
        }

        /// <summary>
        /// Adds Azure Key Vault as a Configuration Source.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="tokenCredential">The token credential to use for authentication.</param>
        /// <returns>The web host builder.</returns>
        public static IWebHostBuilder AddOrchardCoreAzureKeyVault(this IWebHostBuilder builder, TokenCredential tokenCredential)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ConfigureAppConfiguration((context, builder) =>
            {
                // Here 'configuration' is a manager being a builder, and also an 'IConfigurationRoot'
                // if get from the 'context', allowing to get values from the providers already added
                // without having to build a configuration on the fly that would need to be disposed.
                AddOrchardCoreAzureKeyVault(builder, context.Configuration, tokenCredential);
            });

            return builder;
        }

        /// <summary>
        /// Adds Azure Key Vault as a Configuration Source.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <returns>The configuration manager.</returns>
        public static ConfigurationManager AddOrchardCoreAzureKeyVault(this ConfigurationManager configurationManager)
        {
            if (configurationManager is null)
            {
                throw new ArgumentNullException(nameof(configurationManager));
            }

            return configurationManager.AddOrchardCoreAzureKeyVault(new DefaultAzureCredential(includeInteractiveCredentials: true));
        }

        /// <summary>
        /// Adds Azure Key Vault as a Configuration Source.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="tokenCredential">The token credential to use for authentication.</param>
        /// <returns>The configuration manager.</returns>
        public static ConfigurationManager AddOrchardCoreAzureKeyVault(
            this ConfigurationManager configurationManager, TokenCredential tokenCredential)
        {
            if (configurationManager == null)
            {
                throw new ArgumentNullException(nameof(configurationManager));
            }

            // The 'ConfigurationManager' is both an 'IConfigurationBuilder' and an 'IConfigurationRoot'
            // allowing to get data from the current providers without having to build a configuration.
            AddOrchardCoreAzureKeyVault(configurationManager, configurationManager, tokenCredential);

            return configurationManager;
        }

        private static void AddOrchardCoreAzureKeyVault(
            this IConfigurationBuilder builder, IConfiguration configuration, TokenCredential tokenCredential)
        {
            var keyVaultName = configuration["OrchardCore:OrchardCore_KeyVault_Azure:KeyVaultName"];

            TimeSpan? reloadInterval = null;
            if (double.TryParse(configuration["OrchardCore:OrchardCore_KeyVault_Azure:ReloadInterval"], out var interval))
            {
                reloadInterval = TimeSpan.FromSeconds(interval);
            }

            var keyVaultEndpointUri = new Uri("https://" + keyVaultName + ".vault.azure.net");
            var configOptions = new AzureKeyVaultConfigurationOptions()
            {
                Manager = new AzureKeyVaultSecretManager(),
                ReloadInterval = reloadInterval
            };

            builder.AddAzureKeyVault(
                keyVaultEndpointUri,
                tokenCredential,
                configOptions
            );
        }
    }
}
