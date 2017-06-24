using System.Collections.Generic;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Roles
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageRoles = new Permission("ManageRoles", "Managing Roles");
        public static readonly Permission AssignRoles = new Permission("AssignRoles", "Assign Roles", new[] { ManageRoles });

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                ManageRoles, AssignRoles, StandardPermissions.SiteOwner
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageRoles, AssignRoles, StandardPermissions.SiteOwner }
                },
            };
        }

    }
}
