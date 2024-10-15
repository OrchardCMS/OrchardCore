using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;

namespace OrchardCore.Users.TimeZone.Services;

public class UserTimeZoneService : IUserTimeZoneService
{
    private const string CacheKey = "UserTimeZone/";
    private const string EmptyTimeZone = "empty";

    private static readonly DistributedCacheEntryOptions _slidingExpiration = new() { SlidingExpiration = TimeSpan.FromHours(1) };

    private readonly IClock _clock;
    private readonly IDistributedCache _distributedCache;

    public UserTimeZoneService(
        IClock clock,
        IDistributedCache distributedCache)
    {
        _clock = clock;
        _distributedCache = distributedCache;
    }

    /// <inheritdoc/>
    public async ValueTask<ITimeZone> GetAsync(IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var currentTimeZoneId = await GetTimeZoneIdAsync(user);

        if (string.IsNullOrEmpty(currentTimeZoneId))
        {
            return null;
        }

        return _clock.GetTimeZone(currentTimeZoneId);
    }

    /// <inheritdoc/>
    public async ValueTask UpdateAsync(IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrEmpty(user.UserName))
        {
            return;
        }

        await _distributedCache.RemoveAsync(GetCacheKey(user.UserName));
    }

    /// <inheritdoc/>
    private async ValueTask<string> GetTimeZoneIdAsync(IUser user)
    {
        if (string.IsNullOrEmpty(user.UserName))
        {
            return null;
        }

        var key = GetCacheKey(user.UserName);

        var timeZoneId = await _distributedCache.GetStringAsync(key);

        // The timezone is not cached yet, resolve it and store the value
        if (string.IsNullOrEmpty(timeZoneId))
        {
            if (user is User u)
            {
                timeZoneId = u.As<UserTimeZone>()?.TimeZoneId;
            }

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
