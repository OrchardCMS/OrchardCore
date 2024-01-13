using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media
{
    public class MediaCachePermissions : IPermissionProvider
    {
        public static readonly Permission ManageAssetCache = new("ManageAssetCache", "Manage Asset Cache Folder");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
               => Task.FromResult(_allPermissions);

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
            => _allStereotypes;

        private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
        [
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = _allPermissions,
            },
        ];

        private readonly static IEnumerable<Permission> _allPermissions =
        [
            ManageAssetCache,
        ];
    }
}
