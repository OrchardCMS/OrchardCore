using Orchard.Environment.Cache.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Environment.Cache.CacheContextProviders
{
    // TODO: Move this class in Orchard.Roles module

    public class RolesCacheContextProvider : ICacheContextProvider
    {
        public RolesCacheContextProvider()
        {
        }

        public Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            // User is a more generic dependency
            if (contexts.Any(ctx => String.Equals(ctx, "user", StringComparison.OrdinalIgnoreCase)))
            {
                return Task.CompletedTask;
            }

            if (contexts.Any(ctx => String.Equals(ctx, "user.roles", StringComparison.OrdinalIgnoreCase)))
            {
                // TODO: Add actual roles
                entries.Add(new CacheContextEntry("administrator", "0"));

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
