using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Secrets.Azure;

/// <summary>
/// An Azure Key Vault-backed secret store, namespaced by Orchard tenant.
/// </summary>
public class AzureKeyVaultSecretStore : ISecretStore
{
    public const string SecretClientServiceKey = "OrchardCore.Secrets.Azure";

    private const string TenantTag = "OrchardTenant";
    private const string PrefixTag = "OrchardPrefix";
    private const string NameTag = "OrchardName";
    private const string TypeTag = "OrchardType";
    private const string DescriptionTag = "OrchardDescription";
    private const int MaximumVaultNameLength = 127;
    private const int HashSuffixLength = 24;

    private readonly SecretClient _secretClient;
    private readonly string _tenant;
    private readonly string _prefix;
    private readonly IEnumerable<ISecretTypeProvider> _providers;
    private readonly ILogger _logger;

    public AzureKeyVaultSecretStore(
        IOptions<AzureKeyVaultSecretStoreOptions> options,
        ShellSettings shellSettings,
        IEnumerable<ISecretTypeProvider> providers,
        ILogger<AzureKeyVaultSecretStore> logger,
        [FromKeyedServices(SecretClientServiceKey)] SecretClient secretClient)
        : this(secretClient, shellSettings.Name, options.Value.NamePrefix, providers, logger)
    {
    }

    public AzureKeyVaultSecretStore(
        SecretClient secretClient,
        string tenantName,
        string namePrefix,
        IEnumerable<ISecretTypeProvider> providers,
        ILogger<AzureKeyVaultSecretStore> logger)
    {
        ArgumentNullException.ThrowIfNull(secretClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantName);
        ArgumentException.ThrowIfNullOrWhiteSpace(namePrefix);
        if (namePrefix.Length > 256)
        {
            throw new ArgumentException("The Key Vault name prefix must not exceed 256 characters.", nameof(namePrefix));
        }

        _secretClient = secretClient;
        _tenant = tenantName;
        _prefix = namePrefix;
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
        entry.Properties.Tags[PrefixTag] = _prefix;
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
            _logger.LogError("Failed to save secret '{SecretName}' to Azure Key Vault ({ExceptionType}).", name, ex.GetType().Name);
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
            _logger.LogError("Failed to remove secret '{SecretName}' from Azure Key Vault ({ExceptionType}).", name, ex.GetType().Name);
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
                if (!properties.Tags.TryGetValue(PrefixTag, out var prefix) || prefix != _prefix ||
                    !properties.Tags.TryGetValue(TenantTag, out var tenant) || tenant != _tenant ||
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
            _logger.LogError("Failed to list secrets from Azure Key Vault ({ExceptionType}).", ex.GetType().Name);
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
            if (!entry.Properties.Tags.TryGetValue(PrefixTag, out var prefix) || prefix != _prefix ||
                !entry.Properties.Tags.TryGetValue(TenantTag, out var tenant) || tenant != _tenant ||
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
            _logger.LogError("Failed to retrieve secret '{SecretName}' from Azure Key Vault ({ExceptionType}).", name, ex.GetType().Name);
            throw;
        }
    }

    private string GetVaultName(string name)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] { _prefix, _tenant, name }))))[..HashSuffixLength];
        var readableLength = MaximumVaultNameLength - HashSuffixLength - 1;
        var readable = new StringBuilder(readableLength);
        foreach (var character in $"{_prefix}-{_tenant}-{name}")
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                readable.Append(char.ToLowerInvariant(character));
            }
            else if (readable.Length > 0 && readable[^1] != '-')
            {
                readable.Append('-');
            }

            if (readable.Length == readableLength)
            {
                break;
            }
        }

        return $"{readable.ToString().TrimEnd('-')}-{hash}";
    }

    private ISecretTypeProvider GetProvider(string type) =>
        _providers.FirstOrDefault(p => p.Name == type || p.SecretType.FullName == type)
        ?? throw new InvalidOperationException($"Secret type '{type}' is not registered.");

    private static string GetDescription(SecretProperties properties) =>
        properties != null && properties.Tags.TryGetValue(DescriptionTag, out var description) ? description : null;
}
