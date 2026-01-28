using Microsoft.Extensions.Logging;

namespace OrchardCore.Secrets.Services;

/// <summary>
/// Default implementation of <see cref="ISecretManager"/> that manages secrets
/// across multiple stores.
/// </summary>
public class SecretManager : ISecretManager
{
    private readonly IEnumerable<ISecretStore> _stores;
    private readonly ILogger _logger;

    public SecretManager(
        IEnumerable<ISecretStore> stores,
        ILogger<SecretManager> logger)
    {
        _stores = stores;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T> GetSecretAsync<T>(string name) where T : class, ISecret
    {
        foreach (var store in _stores)
        {
            var secret = await store.GetSecretAsync<T>(name);
            if (secret != null)
            {
                return secret;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<T> GetSecretAsync<T>(string name, string storeName) where T : class, ISecret
    {
        var store = GetStore(storeName);
        if (store == null)
        {
            _logger.LogWarning("Secret store '{StoreName}' not found.", storeName);
            return null;
        }

        return await store.GetSecretAsync<T>(name);
    }

    /// <inheritdoc />
    public async Task SaveSecretAsync<T>(string name, T secret, SecretSaveOptions options = null) where T : class, ISecret
    {
        var store = GetDefaultWritableStore();
        if (store == null)
        {
            throw new InvalidOperationException("No writable secret store is available.");
        }

        await store.SaveSecretAsync(name, secret, options);
    }

    /// <inheritdoc />
    public async Task SaveSecretAsync<T>(string name, T secret, string storeName, SecretSaveOptions options = null) where T : class, ISecret
    {
        var store = GetStore(storeName);
        if (store == null)
        {
            throw new InvalidOperationException($"Secret store '{storeName}' not found.");
        }

        if (store.IsReadOnly)
        {
            throw new InvalidOperationException($"Secret store '{storeName}' is read-only.");
        }

        await store.SaveSecretAsync(name, secret, options);
    }

    /// <inheritdoc />
    public async Task RemoveSecretAsync(string name)
    {
        foreach (var store in _stores.Where(s => !s.IsReadOnly))
        {
            await store.RemoveSecretAsync(name);
        }
    }

    /// <inheritdoc />
    public async Task RemoveSecretAsync(string name, string storeName)
    {
        var store = GetStore(storeName);
        if (store == null)
        {
            throw new InvalidOperationException($"Secret store '{storeName}' not found.");
        }

        if (store.IsReadOnly)
        {
            throw new InvalidOperationException($"Secret store '{storeName}' is read-only.");
        }

        await store.RemoveSecretAsync(name);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SecretInfo>> GetSecretInfosAsync()
    {
        var allInfos = new List<SecretInfo>();

        foreach (var store in _stores)
        {
            var infos = await store.GetSecretInfosAsync();
            allInfos.AddRange(infos);
        }

        return allInfos;
    }

    /// <inheritdoc />
    public IEnumerable<ISecretStore> GetStores() => _stores;

    private ISecretStore GetStore(string name) =>
        _stores.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private ISecretStore GetDefaultWritableStore() =>
        _stores.FirstOrDefault(s => !s.IsReadOnly);
}
