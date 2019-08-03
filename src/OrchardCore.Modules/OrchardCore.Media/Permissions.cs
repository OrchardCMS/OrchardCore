using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageMedia = new Permission("ManageMediaContent", "Manage Media");
        public static readonly Permission ManageOwnMedia = new Permission("ManageOwnMedia", "Manage Own Media", new[] { ManageMedia });
        public static readonly Permission ManageAttachedMediaFieldsFolder = new Permission("ManageAttachedMediaFieldsFolder", "Manage Attached Media Fields Folder");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageMedia, ManageOwnMedia, ManageAttachedMediaFieldsFolder };
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
}
