using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageMenu = new Permission("ManageMenu", "Manage menus");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageMenu };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageMenu }
                }
            };
        }
    }
}