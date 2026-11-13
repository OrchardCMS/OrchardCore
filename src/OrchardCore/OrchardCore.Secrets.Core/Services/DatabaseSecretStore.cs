using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Json;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

/// <summary>
/// A database-backed secret store using YesSql and Data Protection for encryption.
/// </summary>
public class DatabaseSecretStore : ISecretStore
{
    private const string DataProtectionPurpose = "OrchardCore.Secrets";

    private readonly IDocumentManager<SecretsDocument> _documentManager;
    private readonly IDataProtector _dataProtector;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;

    public DatabaseSecretStore(
        IDocumentManager<SecretsDocument> documentManager,
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DocumentJsonSerializerOptions> documentJsonSerializerOptions,
        ILogger<DatabaseSecretStore> logger)
    {
        _documentManager = documentManager;
        _dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
        _serializerOptions = documentJsonSerializerOptions.Value.SerializerOptions;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Database";

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public async Task<Secret> GetSecretAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        if (!document.Secrets.TryGetValue(name, out var entry))
        {
            return default;
        }

        if (!Secret.TryGetType(entry.Type, out var type))
        {
            _logger.LogError("The secret '{SecretName}' has an unknown type '{SecretType}'.", name, entry.Type);
            return default;
        }

        try
        {
            var decryptedData = _dataProtector.Unprotect(entry.EncryptedData);

            // The type of the secret is stored with the entry, so the payload is the secret itself.
            return Secret.FromValue(JsonSerializer.Deserialize(decryptedData, type, _serializerOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt secret '{SecretName}'.", name);
            return default;
        }
    }

    /// <inheritdoc />
    public Task SaveSecretAsync(string name, Secret secret, SecretSaveOptions options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(secret.Value, nameof(secret));

        // A separate scope commits only this mutation before a transfer can discard its source.
        return ShellScope.Current == null
            ? SaveCoreAsync(name, secret, options)
            : ShellScope.UsingChildScopeAsync(_ => SaveCoreAsync(name, secret, options), activateShell: false);
    }

    private async Task SaveCoreAsync(string name, Secret secret, SecretSaveOptions options)
    {
        var document = await _documentManager.GetOrCreateMutableAsync();

        var type = secret.Value.GetType();
        var json = JsonSerializer.Serialize(secret.Value, type, _serializerOptions);
        var encryptedData = _dataProtector.Protect(json);

        var now = DateTime.UtcNow;
        var existingEntry = document.Secrets.TryGetValue(name, out var existing) ? existing : null;

        document.Secrets[name] = new SecretEntry
        {
            Name = name,
            Type = type.FullName,
            EncryptedData = encryptedData,
            CreatedUtc = existingEntry?.CreatedUtc ?? now,
            UpdatedUtc = now,
            Description = options == null ? existingEntry?.Description : options.Description,
            ExpiresUtc = options == null ? existingEntry?.ExpiresUtc : options.ExpiresUtc,
        };

        await _documentManager.UpdateAsync(document);
    }

    /// <inheritdoc />
    public Task RemoveSecretAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return ShellScope.Current == null
            ? RemoveCoreAsync(name)
            : ShellScope.UsingChildScopeAsync(_ => RemoveCoreAsync(name), activateShell: false);
    }

    private async Task RemoveCoreAsync(string name)
    {
        var document = await _documentManager.GetOrCreateMutableAsync();

        if (document.Secrets.Remove(name))
        {
            await _documentManager.UpdateAsync(document);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SecretInfo>> GetSecretInfosAsync()
    {
        var document = await _documentManager.GetOrCreateImmutableAsync();

        return document.Secrets.Values.Select(entry => new SecretInfo
        {
            Name = entry.Name,
            Store = Name,
            Type = entry.Type,
            Description = entry.Description,
            CreatedUtc = entry.CreatedUtc,
            UpdatedUtc = entry.UpdatedUtc,
            ExpiresUtc = entry.ExpiresUtc,
        });
    }
}
