using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Secrets.Azure;

/// <summary>
/// An Azure Key Vault-backed secret store, namespaced by Orchard tenant.
/// </summary>
public class AzureKeyVaultSecretStore : ISecretStore
{
    private const string TenantTag = "OrchardTenant";
    private const string NameTag = "OrchardName";
    private const string TypeTag = "OrchardType";
    private const string DescriptionTag = "OrchardDescription";

    private readonly SecretClient _secretClient;
    private readonly string _tenant;
    private readonly IEnumerable<ISecretTypeProvider> _providers;
    private readonly ILogger _logger;

    public AzureKeyVaultSecretStore(
        IOptions<AzureKeyVaultSecretStoreOptions> options,
        ShellSettings shellSettings,
        IEnumerable<ISecretTypeProvider> providers,
        ILogger<AzureKeyVaultSecretStore> logger)
        : this(CreateClient(options.Value), shellSettings.Name, providers, logger)
    {
    }

    public AzureKeyVaultSecretStore(
        SecretClient secretClient,
        string tenantName,
        IEnumerable<ISecretTypeProvider> providers,
        ILogger<AzureKeyVaultSecretStore> logger)
    {
        ArgumentNullException.ThrowIfNull(secretClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantName);

        _secretClient = secretClient;
        _tenant = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(tenantName)));
        _providers = providers;
        _logger = logger;
    }

    public string Name => "AzureKeyVault";

    public bool IsReadOnly => false;

    public async Task<T> GetSecretAsync<T>(string name) where T : class, ISecret
    {
        var secret = await GetEntryAsync(name);
        if (secret == null)
        {
            return null;
        }

        var provider = GetProvider(secret.Properties.Tags[TypeTag]);
        if (!typeof(T).IsAssignableFrom(provider.SecretType))
        {
            return null;
        }

        return (secret.Properties.ContentType == "text/plain" && provider.SecretType == typeof(TextSecret)
            ? new TextSecret { Text = secret.Value }
            : provider.Deserialize(secret.Value)) as T;
    }

    public async Task SaveSecretAsync<T>(string name, T secret, SecretSaveOptions options = null) where T : class, ISecret
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(secret);

        var provider = _providers.FirstOrDefault(p => p.SecretType == secret.GetType())
            ?? throw new InvalidOperationException($"Secret type '{secret.GetType().FullName}' is not registered.");
        var existing = await GetEntryAsync(name);
        var entry = new KeyVaultSecret(GetVaultName(name), secret is TextSecret text ? text.Text : provider.Serialize(secret));
        entry.Properties.ContentType = secret is TextSecret ? "text/plain" : "application/json";
        entry.Properties.Tags[TenantTag] = _tenant;
        entry.Properties.Tags[NameTag] = name;
        entry.Properties.Tags[TypeTag] = provider.Name;

        var description = options == null ? GetDescription(existing?.Properties) : options.Description;
        if (description != null)
        {
            entry.Properties.Tags[DescriptionTag] = description;
        }

        entry.Properties.ExpiresOn = options == null ? existing?.Properties.ExpiresOn : options.ExpiresUtc;

        try
        {
            await _secretClient.SetSecretAsync(entry);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to save secret '{SecretName}' to Azure Key Vault.", name);
            throw;
        }
    }

    public async Task RemoveSecretAsync(string name)
    {
        if (await GetEntryAsync(name) == null)
        {
            return;
        }

        try
        {
            var operation = await _secretClient.StartDeleteSecretAsync(GetVaultName(name));
            await operation.WaitForCompletionAsync();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // A concurrently deleted secret is already absent.
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to remove secret '{SecretName}' from Azure Key Vault.", name);
            throw;
        }
    }

    public async Task<IEnumerable<SecretInfo>> GetSecretInfosAsync()
    {
        var infos = new List<SecretInfo>();
        try
        {
            await foreach (var properties in _secretClient.GetPropertiesOfSecretsAsync())
            {
                if (!properties.Tags.TryGetValue(TenantTag, out var tenant) || tenant != _tenant ||
                    !properties.Tags.TryGetValue(NameTag, out var name) || properties.Name != GetVaultName(name) ||
                    !properties.Tags.TryGetValue(TypeTag, out var type))
                {
                    continue;
                }

                infos.Add(new SecretInfo
                {
                    Name = name,
                    Store = Name,
                    Type = type,
                    Description = GetDescription(properties),
                    CreatedUtc = properties.CreatedOn?.UtcDateTime,
                    UpdatedUtc = properties.UpdatedOn?.UtcDateTime,
                    ExpiresUtc = properties.ExpiresOn?.UtcDateTime,
                });
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to list secrets from Azure Key Vault.");
            throw;
        }

        return infos;
    }

    private async Task<KeyVaultSecret> GetEntryAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        try
        {
            var response = await _secretClient.GetSecretAsync(GetVaultName(name));
            var entry = response.Value;
            if (!entry.Properties.Tags.TryGetValue(TenantTag, out var tenant) || tenant != _tenant ||
                !entry.Properties.Tags.TryGetValue(NameTag, out var originalName) || originalName != name ||
                !entry.Properties.Tags.ContainsKey(TypeTag))
            {
                throw new InvalidOperationException("The Key Vault entry has invalid Orchard secret ownership metadata.");
            }

            return entry;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret '{SecretName}' from Azure Key Vault.", name);
            throw;
        }
    }

    private string GetVaultName(string name) =>
        "oc-" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] { _tenant, name }))));

    private ISecretTypeProvider GetProvider(string type) =>
        _providers.FirstOrDefault(p => p.Name == type || p.SecretType.FullName == type)
        ?? throw new InvalidOperationException($"Secret type '{type}' is not registered.");

    private static string GetDescription(SecretProperties properties) =>
        properties != null && properties.Tags.TryGetValue(DescriptionTag, out var description) ? description : null;

    private static SecretClient CreateClient(AzureKeyVaultSecretStoreOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.VaultUri))
        {
            throw new InvalidOperationException("Azure Key Vault URI is not configured.");
        }

        var vaultUri = new Uri(options.VaultUri);
        return !string.IsNullOrEmpty(options.ClientId) && !string.IsNullOrEmpty(options.ClientSecret)
            ? new SecretClient(vaultUri, new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret))
            : new SecretClient(vaultUri, new DefaultAzureCredential());
    }
}
