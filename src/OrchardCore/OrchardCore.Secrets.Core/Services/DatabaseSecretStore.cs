using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Documents;
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
    public async Task<T> GetSecretAsync<T>(string name) where T : class, ISecret
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        if (!document.Secrets.TryGetValue(name, out var entry))
        {
            return null;
        }

        try
        {
            var decryptedData = _dataProtector.Unprotect(entry.EncryptedData);
            // Deserialize as ISecret to use polymorphic deserialization, then cast to T
            var secret = JsonSerializer.Deserialize<ISecret>(decryptedData, _serializerOptions);
            return secret as T;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt secret '{SecretName}'.", name);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SaveSecretAsync<T>(string name, T secret, SecretSaveOptions options = null) where T : class, ISecret
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(secret);

        var document = await _documentManager.GetOrCreateMutableAsync();

        // Serialize as ISecret to include type discriminator for polymorphic serialization
        var json = JsonSerializer.Serialize<ISecret>(secret, _serializerOptions);
        var encryptedData = _dataProtector.Protect(json);

        var now = DateTime.UtcNow;
        var existingEntry = document.Secrets.TryGetValue(name, out var existing) ? existing : null;

        document.Secrets[name] = new SecretEntry
        {
            Name = name,
            Type = secret.GetType().FullName,
            EncryptedData = encryptedData,
            CreatedUtc = existingEntry?.CreatedUtc ?? now,
            UpdatedUtc = now,
            Description = options?.Description ?? existingEntry?.Description,
            ExpiresUtc = options?.ExpiresUtc ?? existingEntry?.ExpiresUtc,
        };

        await _documentManager.UpdateAsync(document);
    }

    /// <inheritdoc />
    public async Task RemoveSecretAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

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
            CreatedUtc = entry.CreatedUtc,
            UpdatedUtc = entry.UpdatedUtc,
            ExpiresUtc = entry.ExpiresUtc,
        });
    }
}
