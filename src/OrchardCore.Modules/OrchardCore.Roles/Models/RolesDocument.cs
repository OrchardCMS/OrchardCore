using System.Collections.Generic;
using OrchardCore.Data;
using OrchardCore.Security;

namespace OrchardCore.Roles.Models
{
    public class RolesDocument : DistributedDocument
    {
        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
