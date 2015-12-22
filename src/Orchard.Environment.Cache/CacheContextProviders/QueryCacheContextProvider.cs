using Microsoft.AspNet.Http;
using Orchard.Environment.Cache.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Cache.CacheContextProviders
{
    public class QueryCacheContextProvider : ICacheContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QueryCacheContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void PopulateContextEntries(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            if (contexts.Any(ctx => String.Equals(ctx, "query", StringComparison.OrdinalIgnoreCase)))
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var query = httpContext.Request.Query;
                var allKeys = query.Keys.OrderBy(x => x).ToArray();
                entries.AddRange(allKeys
                    .Select(x => new CacheContextEntry(
                        key: x.ToLowerInvariant(), 
                        value: query[x].ToString().ToLowerInvariant())
                    ));
            }
        }
    }
}
