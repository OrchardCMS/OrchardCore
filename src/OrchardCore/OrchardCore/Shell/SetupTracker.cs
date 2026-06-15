using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Distributed;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Environment.Shell;

/// <summary>
/// Default implementation that tracks active tenant setup operations using an in-process
/// dictionary (fast path) and a distributed cache (cross-instance coordination).
/// A distributed lock is acquired briefly for the atomic check-and-write operation,
/// not held for the duration of setup.
/// Registered as a singleton at the host level.
/// </summary>
public sealed class SetupTracker : ISetupTracker
{
    private const string CacheKeyPrefix = "SETUP_IN_PROGRESS_";

    /// <summary>
    /// Expiration for the distributed cache entry. Self-heals if the instance
    /// crashes before calling <see cref="MarkSetupCompletedAsync"/>.
    /// </summary>
    private static readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Timeout for acquiring the brief distributed lock during <see cref="TryMarkSetupStartedAsync"/>.
    /// </summary>
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Expiration for the distributed lock itself (brief, just for atomic check+write).
    /// </summary>
    private static readonly TimeSpan _lockExpiration = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<string, bool> _activeSetups = new(StringComparer.OrdinalIgnoreCase);
    private readonly IShellHost _shellHost;

    public SetupTracker(IShellHost shellHost)
    {
        _shellHost = shellHost;
    }

    /// <inheritdoc />
    public async Task<bool> IsSetupInProgressAsync(ShellSettings shellSettings)
    {
        // Fast path: check local dictionary.
        if (_activeSetups.ContainsKey(shellSettings.Name))
        {
            return true;
        }

        // Distributed path: check the distributed cache for entries from other instances.
        var distributedCache = await GetDistributedCacheAsync();
        if (distributedCache is not null)
        {
            var value = await distributedCache.GetStringAsync(GetCacheKey(shellSettings));
            return value is not null;
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> TryMarkSetupStartedAsync(ShellSettings shellSettings)
    {
        // Fast path: check local dictionary.
        if (_activeSetups.ContainsKey(shellSettings.Name))
        {
            return false;
        }

        var distributedCache = await GetDistributedCacheAsync();

        if (distributedCache is not null)
        {
            // Acquire a brief distributed lock for the atomic check+write operation.
            var distributedLock = await GetDistributedLockAsync();
            if (distributedLock is not null)
            {
                var lockKey = GetLockKey(shellSettings);
                var (locker, locked) = await distributedLock.TryAcquireLockAsync(lockKey, _lockTimeout, _lockExpiration);

                if (!locked)
                {
                    // Another instance is in the process of marking setup as started.
                    return false;
                }

                try
                {
                    // Double-check the cache under the lock.
                    var existing = await distributedCache.GetStringAsync(GetCacheKey(shellSettings));
                    if (existing is not null)
                    {
                        return false;
                    }

                    // Write the "in progress" flag to the distributed cache.
                    await distributedCache.SetStringAsync(
                        GetCacheKey(shellSettings),
                        "1",
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheExpiration,
                        });
                }
                finally
                {
                    // Release the lock immediately — not held for the duration of setup.
                    await locker.DisposeAsync();
                }
            }
        }

        // Add to local dictionary. If another thread beat us, the cache entry still exists
        // and will be cleaned up on expiration.
        if (!_activeSetups.TryAdd(shellSettings.Name, true))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public async Task MarkSetupCompletedAsync(ShellSettings shellSettings)
    {
        _activeSetups.TryRemove(shellSettings.Name, out _);

        // Remove the distributed cache entry so other instances can see it's complete.
        var distributedCache = await GetDistributedCacheAsync();
        if (distributedCache is not null)
        {
            await distributedCache.RemoveAsync(GetCacheKey(shellSettings));
        }
    }

    private async Task<IDistributedCache> GetDistributedCacheAsync()
    {
        if (!_shellHost.TryGetSettings(ShellSettings.DefaultShellName, out var defaultSettings))
        {
            return null;
        }

        var shellContext = await _shellHost.GetOrCreateShellContextAsync(defaultSettings);

        // Only use the distributed cache if the distributed feature is enabled
        // and it's not an in-memory cache (which offers no cross-instance benefit).
        if (shellContext.ServiceProvider.GetService<DistributedShellMarkerService>() is null)
        {
            return null;
        }

        var cache = shellContext.ServiceProvider.GetService<IDistributedCache>();
        if (cache is null || cache is MemoryDistributedCache)
        {
            return null;
        }

        return cache;
    }

    private async Task<IDistributedLock> GetDistributedLockAsync()
    {
        if (!_shellHost.TryGetSettings(ShellSettings.DefaultShellName, out var defaultSettings))
        {
            return null;
        }

        var shellContext = await _shellHost.GetOrCreateShellContextAsync(defaultSettings);
        return shellContext.ServiceProvider.GetService<IDistributedLock>();
    }

    private static string GetCacheKey(ShellSettings shellSettings)
        => $"{CacheKeyPrefix}{shellSettings.Name}";

    private static string GetLockKey(ShellSettings shellSettings)
        => $"SETUP_MARK_LOCK_{shellSettings.Name}";
}
