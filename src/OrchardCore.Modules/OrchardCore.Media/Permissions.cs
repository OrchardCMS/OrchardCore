using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageMedia = new Permission("ManageMediaContent", "Manage Media");
        public static readonly Permission ManageOwnMedia = new Permission("ManageOwnMedia", "Manage Own Media", new[] { ManageMedia });
        public static readonly Permission ManageAttachedMediaFieldsFolder = new Permission("ManageAttachedMediaFieldsFolder", "Manage Attached Media Fields Folder");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageMedia, ManageOwnMedia, ManageAttachedMediaFieldsFolder }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageMedia, ManageAttachedMediaFieldsFolder }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { ManageMedia }
                },
                new PermissionStereotype
                {
                    Name = "Moderator",
                },
                new PermissionStereotype
                {
                    Name = "Author",
                    Permissions = new[] { ManageOwnMedia }
                },
                new PermissionStereotype
                {
                    Name = "Contributor",
                    Permissions = new[] { ManageOwnMedia }
                },
            };
        }
    }

    public class MediaCachePermissions : IPermissionProvider
    {
        public static readonly Permission ManageAssetCache = new Permission("ManageAssetCache", "Manage Asset Cache Folder");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageAssetCache }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAssetCache }
                }
            };
        }
    }
}
