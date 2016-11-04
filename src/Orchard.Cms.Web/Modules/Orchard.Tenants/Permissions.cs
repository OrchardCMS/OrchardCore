using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Tenants
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageTenants = new Permission("ManageTenants", "Manage tenants");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageTenants };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageTenants }
                }
            };
        }
    }
}