using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;

namespace OrchardCore.Users.TimeZone.Services;

/// <summary>
/// Represents a time zone service for currently logged-in user.
/// </summary>
public class UserTimeZoneService : IUserTimeZoneService
{
    private const string CacheKey = "UserTimeZone/";
    private const string EmptyTimeZone = "empty";

    private static readonly DistributedCacheEntryOptions _slidingExpiration = new() { SlidingExpiration = TimeSpan.FromHours(1) };

    private readonly IClock _clock;
    private readonly IDistributedCache _distributedCache;

    public UserTimeZoneService(
        IClock clock,
        IDistributedCache distributedCache,
        IHttpContextAccessor httpContextAccessor)
    {
        _clock = clock;
        _distributedCache = distributedCache;
        HttpContext = httpContextAccessor.HttpContext;
    }

    public HttpContext HttpContext { get; }

    /// <inheritdoc/>
    public async ValueTask<ITimeZone> GetTimeZoneAsync(IUser user)
    {
        var currentTimeZoneId = await GetTimeZoneIdAsync();

        if (string.IsNullOrEmpty(currentTimeZoneId))
        {
            return null;
        }

        return _clock.GetTimeZone(currentTimeZoneId);
    }

    /// <inheritdoc/>
    public async ValueTask UpdateTimeZoneAsync(IUser user)
    {
        var userName = user?.UserName;

        if (!string.IsNullOrEmpty(userName))
        {
            await _distributedCache.RemoveAsync(GetCacheKey(userName));
        }

        return;
    }

    /// <inheritdoc/>
    private async ValueTask<string> GetTimeZoneIdAsync()
    {
        var userName = HttpContext.User?.Identity?.Name;

        if (string.IsNullOrEmpty(userName))
        {
            return null;
        }

        var key = GetCacheKey(userName);

        var timeZoneId = await _distributedCache.GetStringAsync(key);

        // The timezone is not cached yet, resolve it and store the value
        if (string.IsNullOrEmpty(timeZoneId))
        {
            // Delay-loading UserManager since it is registered as scoped
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<IUser>>();
            var user = await userManager.FindByNameAsync(userName) as User;
            timeZoneId = user.As<UserTimeZone>()?.TimeZoneId;

            // We store a special string to remember there is no specific value for this user.
            // And actual distributed cache implementation might not be able to store null values.
            if (string.IsNullOrEmpty(timeZoneId))
            {
                timeZoneId = EmptyTimeZone;
            }

            await _distributedCache.SetStringAsync(key, timeZoneId, _slidingExpiration);
        }

        // Do we know this user doesn't have a configured value?
        if (timeZoneId == EmptyTimeZone)
        {
            return null;
        }

        return timeZoneId;
    }

    private static string GetCacheKey(string userName) => CacheKey + userName;
}
