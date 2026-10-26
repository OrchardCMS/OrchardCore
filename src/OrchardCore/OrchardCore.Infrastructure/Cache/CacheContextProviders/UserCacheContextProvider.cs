using Microsoft.AspNetCore.Http;

namespace OrchardCore.Environment.Cache.CacheContextProviders;

public class UserCacheContextProvider : ICacheContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserCacheContextProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
    {
        if (contexts.ContainsContext("user"))
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.User?.Identity?.IsAuthenticated ?? false)
            {
                entries.Add(new CacheContextEntry("user", httpContext.User.Identity.Name));
            }
            else
            {
                entries.Add(new CacheContextEntry("user", ""));
            }
        }

        return Task.CompletedTask;
    }
}
