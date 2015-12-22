using Microsoft.AspNet.Http;
using Orchard.Environment.Cache.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Cache.CacheContextProviders
{
    public class RouteCacheContextProvider : ICacheContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RouteCacheContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void PopulateContextEntries(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            if (contexts.Any(ctx => String.Equals(ctx, "route", StringComparison.OrdinalIgnoreCase)))
            {
                var httpContext = _httpContextAccessor.HttpContext;
                entries.Add(new CacheContextEntry("route", httpContext.Request.Path.ToString().ToLowerInvariant()));
            }
        }
    }
}
