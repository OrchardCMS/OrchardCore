using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.TimeZone.Models;

namespace OrchardCore.Users.TimeZone.Services;

public class UserTimeZoneService : IUserTimeZoneService
{
    private const string CacheKey = "UserTimeZone/";
    private readonly TimeSpan _slidingExpiration = TimeSpan.FromMinutes(1);

    private readonly IClock _clock;
    private readonly IDistributedCache _distributedCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IUser> _userManager;

    public UserTimeZoneService(
        IClock clock,
        IDistributedCache distributedCache,
        IHttpContextAccessor httpContextAccessor,
        UserManager<IUser> userManager
    )
    {
        _clock = clock;
        _distributedCache = distributedCache;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<ITimeZone> GetUserTimeZoneAsync()
    {
        var currentTimeZoneId = await GetCurrentUserTimeZoneIdAsync();

        if (string.IsNullOrEmpty(currentTimeZoneId))
        {
            return null;
        }

        return _clock.GetTimeZone(currentTimeZoneId);
    }

    public async Task UpdateUserTimeZoneAsync(IUser user)
    {
        var userName = user?.UserName;

        if (!string.IsNullOrEmpty(userName))
        {
            await _distributedCache.RemoveAsync(GetCacheKey(userName));
        }

        return;
    }

    public async Task<string> GetCurrentUserTimeZoneIdAsync()
    {
        var userName = _httpContextAccessor.HttpContext.User?.Identity?.Name;

        if (string.IsNullOrEmpty(userName))
        {
            return null;
        }

        var key = GetCacheKey(userName);
        var timeZoneId = await _distributedCache.GetStringAsync(key);

        if (timeZoneId is null)
        {
            var user = await _userManager.FindByNameAsync(userName) as User;
            timeZoneId = user.As<UserTimeZone>()?.TimeZoneId;

            await _distributedCache.SetStringAsync(
                key,
                timeZoneId,
                new DistributedCacheEntryOptions { SlidingExpiration = _slidingExpiration }
            );
        }

        return timeZoneId;
    }

    private static string GetCacheKey(string userName) => CacheKey + userName;
}
