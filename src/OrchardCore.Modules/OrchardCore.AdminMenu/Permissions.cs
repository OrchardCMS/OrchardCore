using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu
{
    // todo
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAdminMenu = new Permission("ManageAdminMenu", "Manage the admin menu");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageAdminMenu };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAdminMenu }
                }
            };
        }
    }
}