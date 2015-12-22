using Orchard.Environment.Cache.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Cache.CacheContextProviders
{
    // TODO: Move this class in Orchard.Roles module

    public class RolesCacheContextProvider : ICacheContextProvider
    {
        public RolesCacheContextProvider()
        {
        }

        public void PopulateContextEntries(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            if (contexts.Any(ctx => String.Equals(ctx, "roles", StringComparison.OrdinalIgnoreCase)))
            {
                entries.Add(new CacheContextEntry("administrator", "0"));
            }
        }
    }
}
