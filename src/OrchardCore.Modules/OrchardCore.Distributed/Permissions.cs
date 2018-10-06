using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Distributed
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageDistributedServices =
            new Permission("ManageDistributedServices", "Manage Distributed Services");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageDistributedServices };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageDistributedServices }
                }
            };
        }
    }
}