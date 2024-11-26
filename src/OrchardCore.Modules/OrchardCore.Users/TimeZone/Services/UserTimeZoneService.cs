using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Handlers;
using OrchardCore.Users.TimeZone.Models;

namespace OrchardCore.Users.TimeZone.Services;

public class UserTimeZoneService : IUserTimeZoneService
{
    private const string EmptyTimeZone = "NoTimeZoneFound";

    private static readonly DistributedCacheEntryOptions _slidingExpiration = new()
    {
        SlidingExpiration = TimeSpan.FromHours(1),
    };

    private readonly IClock _clock;
    private readonly IDistributedCache _distributedCache;
    private readonly UserManager<IUser> _userManager;

    public UserTimeZoneService(
        IClock clock,
        IDistributedCache distributedCache,
        UserManager<IUser> userManager)
    {
        _clock = clock;
        _distributedCache = distributedCache;
        _userManager = userManager;
    }

    /// <inheritdoc/>
    public async ValueTask<ITimeZone> GetAsync(string userName)
    {
        ArgumentException.ThrowIfNullOrEmpty(userName);

        var currentTimeZoneId = await GetTimeZoneIdAsync(userName);

        if (string.IsNullOrEmpty(currentTimeZoneId))
        {
            return null;
        }

        return _clock.GetTimeZone(currentTimeZoneId);
    }

    /// <inheritdoc/>
    public ValueTask<ITimeZone> GetAsync(IUser user)
        => GetAsync(user?.UserName);

    /// <inheritdoc/>
    public async ValueTask UpdateAsync(IUser user)
        => await ForgetCacheAsync(user?.UserName);

    /// <inheritdoc/>
    private async ValueTask<string> GetTimeZoneIdAsync(string userName)
    {
        var key = UserEventHandler.GetCacheKey(userName);

        var timeZoneId = await _distributedCache.GetStringAsync(key);

        // The timeZone is not cached yet, resolve it and store the value.
        if (string.IsNullOrEmpty(timeZoneId))
        {
            // At this point, we know the timeZoneId is not cached for the given userName.
            // Retrieve the user and cache the timeZoneId.
            var user = await _userManager.FindByNameAsync(userName);

            if (user is User u)
            {
                timeZoneId = u.As<UserTimeZone>()?.TimeZoneId;
            }

            // We store a placeholder string to indicate that there is no specific value for this user.
            // This approach ensures compatibility with distributed cache implementations that may not support null values.
            // Caching this placeholder helps avoid redundant queries for this user on each request when no time zone is set.
            if (string.IsNullOrEmpty(timeZoneId))
            {
                timeZoneId = EmptyTimeZone;
            }

            await _distributedCache.SetStringAsync(key, timeZoneId, _slidingExpiration);
        }

        // If TimeZoneId matches the placeholder value, we return null instead of the placeholder itself.
        if (timeZoneId == EmptyTimeZone)
        {
            return null;
        }

        return timeZoneId;
    }

    private Task ForgetCacheAsync(string userName)
    {
        var key = UserEventHandler.GetCacheKey(userName);

        return _distributedCache.RemoveAsync(key);
    }
}
