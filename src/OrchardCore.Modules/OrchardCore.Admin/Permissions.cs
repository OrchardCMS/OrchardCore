using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission AccessAdminPanel = new Permission("AccessAdminPanel", "Access admin panel");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                AccessAdminPanel
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = GetPermissions()
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = GetPermissions()
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = GetPermissions()
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = GetPermissions()
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = GetPermissions()
                }
            };
        }
    }
}