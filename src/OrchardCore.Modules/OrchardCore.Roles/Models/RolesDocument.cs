using System.Collections.Generic;
using OrchardCore.Security;
using OrchardCore.Infrastructure.Cache;

namespace OrchardCore.Roles.Models
{
    public class RolesDocument : IScopedDistributedCacheable
    {
        public int Id { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public string CacheId { get; set; }
    }
}
