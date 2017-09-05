using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageSettings = new Permission("ManageSettings", "Manage settings");

        // This permission is not exposed, it's just used for the APIs to generate/check custom ones
        public static readonly Permission ManageGroupSettings = new Permission("ManageResourceSettings", "Manage settings", new[] { ManageSettings });

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageSettings };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageSettings }
                }
            };
        }
    }
}