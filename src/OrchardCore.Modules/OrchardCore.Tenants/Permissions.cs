using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tenants
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