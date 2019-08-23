using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.Azure
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAzureAssetCache = new Permission("ManageAzureAssetCache", "Manage Azure Storage Asset Cache Folder");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageAzureAssetCache };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAzureAssetCache }
                }
            };
        }
    }
}
