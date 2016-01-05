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
            // User is a more generic dependency
            if (contexts.Any(ctx => String.Equals(ctx, "user", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (contexts.Any(ctx => String.Equals(ctx, "user.roles", StringComparison.OrdinalIgnoreCase)))
            {
                // TODO: Add actual roles
                entries.Add(new CacheContextEntry("administrator", "0"));
            }
        }
    }
}
