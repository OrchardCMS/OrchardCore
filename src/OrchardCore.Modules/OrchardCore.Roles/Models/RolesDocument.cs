using System.Collections.Generic;
using OrchardCore.Data;
using OrchardCore.Security;

namespace OrchardCore.Roles.Models
{
    public class RolesDocument : DistributedCacheData
    {
        public int Id { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
