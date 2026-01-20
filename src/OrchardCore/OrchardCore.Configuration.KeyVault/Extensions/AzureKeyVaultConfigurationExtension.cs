using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrchardCore.Configuration.KeyVault.Services;

namespace OrchardCore.Configuration.KeyVault.Extensions;

public static class AzureKeyVaultConfigurationExtension
{
    /// <summary>
    /// Adds Azure Key Vault as a Configuration Source.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="tokenCredential">The token credential to use for authentication.</param>
    /// <returns>The host builder.</returns>
    public static IHostBuilder AddOrchardCoreAzureKeyVault(this IHostBuilder builder, TokenCredential tokenCredential = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureAppConfiguration((context, builder) =>
        {
            // Here 'builder' is a config manager being a builder and also an 'IConfigurationRoot'
            // if get from the 'context', allowing to get values from the providers already added
            // without having to build a configuration on the fly that would need to be disposed.
            AddOrchardCoreAzureKeyVault(builder, context.Configuration, tokenCredential);
        });

        return builder;
    }

    /// <summary>
    /// Adds Azure Key Vault as a Configuration Source.
    /// </summary>
    public static IWebHostBuilder AddOrchardCoreAzureKeyVault(this IWebHostBuilder builder, TokenCredential tokenCredential = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureAppConfiguration((context, builder) =>
        {
            // Here 'builder' is a config manager being a builder and also an 'IConfigurationRoot'
            // if get from the 'context', allowing to get values from the providers already added
            // without having to build a configuration on the fly that would need to be disposed.
            AddOrchardCoreAzureKeyVault(builder, context.Configuration, tokenCredential);
        });

        return builder;
    }

    /// <summary>
    /// Adds Azure Key Vault as a Configuration Source.
    /// </summary>
    public static ConfigurationManager AddOrchardCoreAzureKeyVault(
        this ConfigurationManager manager, TokenCredential tokenCredential = null)
    {
        ArgumentNullException.ThrowIfNull(manager);

        // The 'ConfigurationManager' is a builder and also an 'IConfigurationRoot' allowing to
        // get values from the providers already added without having to build a configuration.
        AddOrchardCoreAzureKeyVault(manager, manager, tokenCredential);

        return manager;
    }

    private static void AddOrchardCoreAzureKeyVault(
        this IConfigurationBuilder builder, IConfiguration configuration, TokenCredential tokenCredential)
    {
        var keyVaultEndpointUri = GetVaultHostUri(configuration);

        var configOptions = new AzureKeyVaultConfigurationOptions()
        {
            Manager = new AzureKeyVaultSecretManager(),
        };

        if (double.TryParse(configuration["OrchardCore:OrchardCore_KeyVault_Azure:ReloadInterval"], out var interval))
        {
            configOptions.ReloadInterval = TimeSpan.FromSeconds(interval);
        }

        tokenCredential ??= new DefaultAzureCredential(includeInteractiveCredentials: true);

        builder.AddAzureKeyVault(keyVaultEndpointUri, tokenCredential, configOptions);
    }

    private static Uri GetVaultHostUri(IConfiguration configuration)
    {
        var vaultUri = configuration["OrchardCore:OrchardCore_KeyVault_Azure:VaultURI"];

        if (!string.IsNullOrWhiteSpace(vaultUri))
        {
            if (!Uri.TryCreate(vaultUri, UriKind.Absolute, out var uri))
            {
                throw new Exception("Invalid value used for 'VaultURI' property. Please provide a valid vault host name using the 'OrchardCore:OrchardCore_KeyVault_Azure:VaultURI' settings key.");
            }

            return uri;
        }

        var keyVaultName = configuration["OrchardCore:OrchardCore_KeyVault_Azure:KeyVaultName"];

        if (string.IsNullOrEmpty(keyVaultName))
        {
            throw new Exception("The 'KeyVaultName' property is not configured. Please configure it by specifying the 'OrchardCore:OrchardCore_KeyVault_Azure:KeyVaultName' settings key.");
        }

        if (!Uri.TryCreate($"https://{keyVaultName}.vault.azure.net", UriKind.Absolute, out var host))
        {
            throw new Exception("Invalid value used for 'KeyVaultName' property. Please provide a valid key-vault name using the 'OrchardCore:OrchardCore_KeyVault_Azure:KeyVaultName' settings key.");
        }

        return host;
    }
}
