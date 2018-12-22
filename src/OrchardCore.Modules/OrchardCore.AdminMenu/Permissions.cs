using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu
{
    // todo
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAdminTree = new Permission("ManageAdminTree", "Manage the admin tree");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageAdminTree };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAdminTree }
                }
            };
        }
    }
}