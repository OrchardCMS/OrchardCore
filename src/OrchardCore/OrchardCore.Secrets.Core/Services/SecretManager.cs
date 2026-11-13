using Microsoft.Extensions.Logging;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Secrets.Services;

/// <summary>
/// Default implementation of <see cref="ISecretManager"/> that manages secrets
/// across multiple stores.
/// </summary>
public class SecretManager : ISecretManager
{
    private readonly IEnumerable<ISecretStore> _stores;
    private readonly ILogger _logger;
    private readonly IDistributedLock _distributedLock;

    public SecretManager(
        IEnumerable<ISecretStore> stores,
        ILogger<SecretManager> logger,
        IDistributedLock distributedLock)
    {
        _stores = stores;
        _logger = logger;
        _distributedLock = distributedLock;
    }

    /// <inheritdoc />
    public async Task<Secret> GetSecretAsync(string name)
    {
        foreach (var store in _stores)
        {
            var secret = await store.GetSecretAsync(name);
            if (secret is not null)
            {
                return secret;
            }
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<Secret> GetSecretAsync(string name, string storeName)
    {
        var store = GetStore(storeName);
        if (store == null)
        {
            _logger.LogWarning("Secret store '{StoreName}' not found.", storeName);
            return default;
        }

        return await store.GetSecretAsync(name);
    }

    /// <inheritdoc />
    public async Task SaveSecretAsync(string name, Secret secret, SecretSaveOptions options = null)
    {
        await using var locker = await SecretStoreOperations.AcquireMutationLockAsync(_distributedLock);
        var store = GetDefaultWritableStore();
        if (store == null)
        {
            throw new InvalidOperationException("No writable secret store is available.");
        }

        await store.SaveSecretAsync(name, secret, options);
    }

    /// <inheritdoc />
    public async Task SaveSecretAsync(string name, Secret secret, string storeName, SecretSaveOptions options = null)
    {
        await using var locker = await SecretStoreOperations.AcquireMutationLockAsync(_distributedLock);
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
        await using var locker = await SecretStoreOperations.AcquireMutationLockAsync(_distributedLock);
        foreach (var store in _stores.Where(s => !s.IsReadOnly))
        {
            await store.RemoveSecretAsync(name);
        }
    }

    /// <inheritdoc />
    public async Task RemoveSecretAsync(string name, string storeName)
    {
        await using var locker = await SecretStoreOperations.AcquireMutationLockAsync(_distributedLock);
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
