using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.Azure
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAzureMediaCache = new Permission("ManageAzureMediaCache", "Manage Azure Media Cache Folder");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageAzureMediaCache };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAzureMediaCache }
                }
            };
        }
    }
}
