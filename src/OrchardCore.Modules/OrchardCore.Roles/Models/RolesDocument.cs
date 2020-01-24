using System.Collections.Generic;
using OrchardCore.Security;
using OrchardCore.Infrastructure.Cache;

namespace OrchardCore.Roles.Models
{
    public class RolesDocument : ScopedDistributedCacheEntry
    {
        public int Id { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
