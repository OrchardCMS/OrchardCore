using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Orchard.Modules
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageFeatures = new Permission("ManageFeatures") { Description = "Manage Features" };

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] 
            {
                ManageFeatures
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageFeatures }
                }
            };
        }
    }
}