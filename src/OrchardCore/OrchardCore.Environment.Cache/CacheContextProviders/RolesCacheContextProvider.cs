using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Environment.Cache.CacheContextProviders
{
    public class RolesCacheContextProvider : ICacheContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RolesCacheContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            if (contexts.Any(ctx => String.Equals(ctx, "user.roles", StringComparison.OrdinalIgnoreCase)))
            {
                var user = _httpContextAccessor.HttpContext.User;
                if (user.Identity.IsAuthenticated)
                {
                    var roleClaims = user.Claims.Where(x => x.Type == ClaimTypes.Role);
                    foreach (var roleClaim in roleClaims)
                    {
                        entries.Add(new CacheContextEntry("user.roles", roleClaim.Value));
                    }

                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }
    }
}
