using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Settings
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageSettings = new Permission("ManageSettings", "Manage settings");
        public static readonly Permission Restart = new Permission("Restart", "Restart the current site");
        

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