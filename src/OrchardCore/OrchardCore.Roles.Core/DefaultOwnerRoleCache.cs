using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Infrastructure.Security;
using OrchardCore.Security;

namespace OrchardCore.Roles.Core;

public class DefaultOwnerRoleCache : IOwnerRoleCache
{
    private const string _cacheKey = "OWNER_ROLES_CACHE";

    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly RoleManager<IRole> _roleManager;
    private readonly SemaphoreSlim _semaphore = new(1);

    private HashSet<string> _ownerRoles;

    public DefaultOwnerRoleCache(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        RoleManager<IRole> roleManager)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _roleManager = roleManager;
        memoryCache.TryGetValue(_cacheKey, out _ownerRoles);
    }

    public async ValueTask<IReadOnlySet<string>> GetAsync()
    {
        if (_ownerRoles is null)
        {
            await InitializeAsync();
        }

        return _ownerRoles;
    }

    public async ValueTask AddAsync(IRole role)
    {
        if (_ownerRoles is null)
        {
            await InitializeAsync();
        }

        if (_ownerRoles.Contains(role.RoleName))
        {
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            _ownerRoles.Add(role.RoleName);

            await SaveAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask RemoveAsync(IRole role)
    {
        if (_ownerRoles is null)
        {
            await InitializeAsync();
        }

        if (!_ownerRoles.Contains(role.RoleName))
        {
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            _ownerRoles.Remove(role.RoleName);

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
            var data = await _distributedCache.GetAsync(_cacheKey);

            _ownerRoles = new(StringComparer.OrdinalIgnoreCase);

            if (data is not null)
            {
                // At this point, we know that roles are already cached. Load from the cache.
                var items = JsonSerializer.Deserialize<string[]>(data);

                foreach (var item in items)
                {
                    _ownerRoles.Add(item);
                }

                // No need to update the distributed cache, but update the memory cache.
                _memoryCache.Set(_cacheKey, _ownerRoles);
            }
            else
            {
                // At this point, the roles were never cached, load from the role manager.
                var roles = _roleManager.Roles.ToList();

                foreach (var role in roles)
                {
                    if (role.Type == RoleType.Owner)
                    {
                        continue;
                    }

                    _ownerRoles.Add(role.RoleName);
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
        _memoryCache.Set(_cacheKey, _ownerRoles);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(_ownerRoles);

        return _distributedCache.SetAsync(_cacheKey, bytes);
    }
}
