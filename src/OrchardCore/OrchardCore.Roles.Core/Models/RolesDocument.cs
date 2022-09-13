using System.Collections.Generic;
using OrchardCore.Data.Documents;
using OrchardCore.Security;

namespace OrchardCore.Roles.Models
{
    [YesSql.SimplifiedTypeName("OrchardCore.Roles.Models.RolesDocument, OrchardCore.Roles")]
    public class RolesDocument : Document
    {
        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
