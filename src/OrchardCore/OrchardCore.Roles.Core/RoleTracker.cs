using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Security;

namespace OrchardCore.Roles.Core;

public class RoleTracker : IRoleTracker
{
    private const string _roleTrackerCacheKey = "ROLES_WITH_FULL_ACCESS_TRACKER";

    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly RoleManager<IRole> _roleManager;

    private HashSet<string> _roleWithFullAccess;

    private readonly SemaphoreSlim _semaphore = new(1);

    public RoleTracker(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        RoleManager<IRole> roleManager)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _roleManager = roleManager;
        memoryCache.TryGetValue(_roleWithFullAccess, out _roleWithFullAccess);
    }

    public async Task<IReadOnlySet<string>> GetAsync()
    {
        if (_roleWithFullAccess is null)
        {
            await InitializeAsync();
        }

        return _roleWithFullAccess;
    }

    public async Task AddAsync(IRole role)
    {
        if (_roleWithFullAccess is null)
        {
            await InitializeAsync();
        }

        if (_roleWithFullAccess.Contains(role.RoleName))
        {
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            _roleWithFullAccess.Add(role.RoleName);

            await SaveAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RemoveAsync(IRole role)
    {
        if (_roleWithFullAccess is null)
        {
            await InitializeAsync();
        }

        if (!_roleWithFullAccess.Contains(role.RoleName))
        {
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            _roleWithFullAccess.Remove(role.RoleName);

            await SaveAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task InitializeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var data = await _distributedCache.GetAsync(_roleTrackerCacheKey);

            _roleWithFullAccess = new(StringComparer.OrdinalIgnoreCase);

            if (data is not null)
            {
                // At this point, we know that roles are already cached. Load from the cache.
                var items = JsonSerializer.Deserialize<string[]>(data);

                foreach (var item in items)
                {
                    _roleWithFullAccess.Add(item);
                }

                // No need to update the distributed cache, but update the memory cache.
                _memoryCache.Set(_roleTrackerCacheKey, _roleWithFullAccess);
            }
            else
            {
                // At this point, the roles were never cached, load from the role manager.
                var roles = _roleManager.Roles.ToList();

                foreach (var role in roles)
                {
                    if (!role.HasFullAccess)
                    {
                        continue;
                    }

                    _roleWithFullAccess.Add(role.RoleName);
                }

                // Update the memory and the distributed cache.
                await SaveAsync();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private Task SaveAsync()
    {
        _memoryCache.Set(_roleTrackerCacheKey, _roleWithFullAccess);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(_roleWithFullAccess);

        return _distributedCache.SetAsync(_roleTrackerCacheKey, bytes);
    }
}
