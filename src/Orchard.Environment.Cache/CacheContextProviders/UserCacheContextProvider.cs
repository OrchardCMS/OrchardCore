using Microsoft.AspNet.Http;
using Orchard.Environment.Cache.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Cache.CacheContextProviders
{
    public class UserCacheContextProvider : ICacheContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserCacheContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void PopulateContextEntries(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            if (contexts.Any(ctx => String.Equals(ctx, "user", StringComparison.OrdinalIgnoreCase)))
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext.User == null)
                {
                    entries.Add(new CacheContextEntry("user", "anonymous"));
                }
                else
                {
                    entries.Add(new CacheContextEntry("user", httpContext.User.Identity.Name));
                }
            }
        }
    }
}
