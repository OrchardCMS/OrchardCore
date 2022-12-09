using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.Models
{
    public class RolesDocument : Document
    {
        public List<Role> Roles { get; set; } = new List<Role>();


        /// <summary>
        /// Keeps track of all permission that were automaticly assigned to a role using <see cref="IPermissionProvider"></see>/>
        /// </summary>
        public Dictionary<string, List<string>> PermissionGroups { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
