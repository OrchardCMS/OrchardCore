using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Settings
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageSettings = new Permission("ManageSettings", "Manage settings");
        public static readonly Permission Restart = new Permission("Restart", "Restart the current site");
        

        // This permission is not exposed, it's just used for the APIs to generate/check custom ones
        public static readonly Permission ManageGroupSettings = new Permission("ManageResourceSettings", "Manage settings", new[] { ManageSettings });

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageSettings, Restart };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageSettings, Restart }
                }
            };
        }
    }
}