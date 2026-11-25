using System.ComponentModel.DataAnnotations;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Secrets.AzureKeyVault;

/// <summary>
/// Validates the <see cref="AzureKeyVaultSecretStoreOptions"/> of a tenant.
/// </summary>
internal static class AzureKeyVaultSecretStoreOptionsExtension
{
    private const string ConfigurationSection = "Secrets:AzureKeyVault";
    private const int MaximumNamePrefixLength = 256;

    /// <summary>
    /// Returns an error when <see cref="AzureKeyVaultSecretStoreOptions.AzureClient"/> doesn't name a keyed
    /// <see cref="SecretClient"/> registered in <paramref name="services"/>, or when
    /// <see cref="AzureKeyVaultSecretStoreOptions.NamePrefix"/> is blank or too long.
    /// </summary>
    public static IEnumerable<ValidationResult> Validate(this AzureKeyVaultSecretStoreOptions options, IServiceCollection services)
    {
        if (string.IsNullOrWhiteSpace(options.AzureClient))
        {
            yield return new ValidationResult($"'{ConfigurationSection}:AzureClient' is not configured.");
        }
        else if (options.AzureClient == AzureKeyVaultSecretStore.SecretClientServiceKey)
        {
            yield return new ValidationResult($"'{ConfigurationSection}:AzureClient' must not be the feature's own service key '{AzureKeyVaultSecretStore.SecretClientServiceKey}'.");
        }
        else if (!services.Any(descriptor => descriptor.ServiceType == typeof(SecretClient) && descriptor.IsKeyedService && Equals(descriptor.ServiceKey, options.AzureClient)))
        {
            yield return new ValidationResult($"The host has no SecretClient registered with the service key '{options.AzureClient}'.");
        }

        if (string.IsNullOrWhiteSpace(options.NamePrefix))
        {
            yield return new ValidationResult($"'{ConfigurationSection}:NamePrefix' must not be blank.");
        }
        else if (options.NamePrefix.Length > MaximumNamePrefixLength)
        {
            yield return new ValidationResult($"'{ConfigurationSection}:NamePrefix' must not exceed {MaximumNamePrefixLength} characters.");
        }
    }
}
