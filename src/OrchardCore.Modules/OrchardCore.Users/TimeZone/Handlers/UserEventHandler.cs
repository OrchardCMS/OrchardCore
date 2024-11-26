using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Users.Handlers;

namespace OrchardCore.Users.TimeZone.Handlers;

public class UserEventHandler : UserEventHandlerBase
{
    private const string CacheKey = "UserTimeZone/";

    private readonly IDistributedCache _distributedCache;

    public UserEventHandler(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public override Task DeletedAsync(UserDeleteContext context)
        => ForgetCacheAsync(context.User.UserName);

    public override Task UpdatedAsync(UserUpdateContext context)
        => ForgetCacheAsync(context.User.UserName);

    public override Task DisabledAsync(UserContext context)
        => ForgetCacheAsync(context.User.UserName);

    private Task ForgetCacheAsync(string userName)
    {
        var key = GetCacheKey(userName);

        return _distributedCache.RemoveAsync(key);
    }

    internal static string GetCacheKey(string userName)
        => CacheKey + userName;
}
