using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Https
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageHttps = new Permission("ManageHttps", "Manage HTTPS");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageHttps };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageHttps }
                }
            };
        }
    }
}