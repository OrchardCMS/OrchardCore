using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Layers
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageLayers = new Permission("ManageLayers", "Manage layers");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageLayers };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageLayers }
                }
            };
        }
    }
}