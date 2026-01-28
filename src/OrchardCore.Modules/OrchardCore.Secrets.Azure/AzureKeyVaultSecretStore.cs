using System.Text.Json;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Secrets.Azure;

/// <summary>
/// An Azure Key Vault-backed secret store.
/// </summary>
public class AzureKeyVaultSecretStore : ISecretStore
{
    private readonly SecretClient _secretClient;
    private readonly ILogger _logger;

    public AzureKeyVaultSecretStore(
        IOptions<AzureKeyVaultSecretStoreOptions> options,
        ILogger<AzureKeyVaultSecretStore> logger)
    {
        _logger = logger;

        var opts = options.Value;

        if (string.IsNullOrEmpty(opts.VaultUri))
        {
            _logger.LogWarning("Azure Key Vault URI is not configured. The store will not be functional.");
            return;
        }

        var vaultUri = new Uri(opts.VaultUri);

        if (!string.IsNullOrEmpty(opts.ClientId) && !string.IsNullOrEmpty(opts.ClientSecret))
        {
            var credential = new ClientSecretCredential(opts.TenantId, opts.ClientId, opts.ClientSecret);
            _secretClient = new SecretClient(vaultUri, credential);
        }
        else
        {
            // Use DefaultAzureCredential which supports managed identities, VS, Azure CLI, etc.
            _secretClient = new SecretClient(vaultUri, new DefaultAzureCredential());
        }
    }

    /// <inheritdoc />
    public string Name => "AzureKeyVault";

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public async Task<T> GetSecretAsync<T>(string name) where T : class, ISecret
    {
        if (_secretClient == null)
        {
            return null;
        }

        try
        {
            // Key Vault doesn't allow certain characters in names, replace them
            var sanitizedName = SanitizeSecretName(name);

            var response = await _secretClient.GetSecretAsync(sanitizedName);
            var secretValue = response.Value.Value;

            // For TextSecret, wrap the value
            if (typeof(T) == typeof(TextSecret))
            {
                return new TextSecret { Text = secretValue } as T;
            }

            // For other types, try to deserialize from JSON
            return JsonSerializer.Deserialize<T>(secretValue);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Secret not found
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret '{SecretName}' from Azure Key Vault.", name);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SaveSecretAsync<T>(string name, T secret, SecretSaveOptions options = null) where T : class, ISecret
    {
        if (_secretClient == null)
        {
            throw new InvalidOperationException("Azure Key Vault is not configured.");
        }

        try
        {
            var sanitizedName = SanitizeSecretName(name);
            string secretValue;

            // For TextSecret, store the text directly
            if (secret is TextSecret textSecret)
            {
                secretValue = textSecret.Text;
            }
            else
            {
                // For other types, serialize to JSON
                secretValue = JsonSerializer.Serialize(secret);
            }

            var keyVaultSecret = new KeyVaultSecret(sanitizedName, secretValue);

            // Apply expiration if specified
            if (options?.ExpiresUtc.HasValue == true)
            {
                keyVaultSecret.Properties.ExpiresOn = options.ExpiresUtc.Value;
            }

            await _secretClient.SetSecretAsync(keyVaultSecret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save secret '{SecretName}' to Azure Key Vault.", name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoveSecretAsync(string name)
    {
        if (_secretClient == null)
        {
            return;
        }

        try
        {
            var sanitizedName = SanitizeSecretName(name);
            var operation = await _secretClient.StartDeleteSecretAsync(sanitizedName);

            // Wait for deletion to complete
            await operation.WaitForCompletionAsync();

            // Purge the deleted secret (permanent deletion)
            await _secretClient.PurgeDeletedSecretAsync(sanitizedName);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Secret not found, nothing to delete
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove secret '{SecretName}' from Azure Key Vault.", name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SecretInfo>> GetSecretInfosAsync()
    {
        if (_secretClient == null)
        {
            return [];
        }

        var infos = new List<SecretInfo>();

        try
        {
            await foreach (var secretProperties in _secretClient.GetPropertiesOfSecretsAsync())
            {
                infos.Add(new SecretInfo
                {
                    Name = secretProperties.Name,
                    Store = Name,
                    Type = nameof(TextSecret), // Key Vault stores strings, we default to TextSecret
                    CreatedUtc = secretProperties.CreatedOn?.UtcDateTime,
                    UpdatedUtc = secretProperties.UpdatedOn?.UtcDateTime,
                    ExpiresUtc = secretProperties.ExpiresOn?.UtcDateTime,
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list secrets from Azure Key Vault.");
        }

        return infos;
    }

    /// <summary>
    /// Sanitizes a secret name to be compatible with Azure Key Vault naming requirements.
    /// Key Vault secret names can only contain alphanumeric characters and dashes.
    /// </summary>
    private static string SanitizeSecretName(string name)
    {
        // Replace underscores and other invalid characters with dashes
        return name.Replace('_', '-').Replace('.', '-').Replace(':', '-');
    }
}
