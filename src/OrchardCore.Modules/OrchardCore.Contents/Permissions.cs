using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents
{
    public class Permissions : IPermissionProvider
    {
        // Note - in code you should demand PublishContent, EditContent, or DeleteContent.
        // Do not demand the "Own" variations - those are applied automatically when you demand the main ones.

        // EditOwn is the permission that is ultimately required to create new content. See how the Create() method is implemented in the AdminController.

        public static readonly Permission PublishContent = CommonPermissions.PublishContent;
        public static readonly Permission PublishOwnContent = CommonPermissions.PublishOwnContent;
        public static readonly Permission EditContent = CommonPermissions.EditContent;
        public static readonly Permission EditOwnContent = CommonPermissions.EditOwnContent;
        public static readonly Permission DeleteContent = CommonPermissions.DeleteContent;
        public static readonly Permission DeleteOwnContent = CommonPermissions.DeleteOwnContent;
        public static readonly Permission ViewContent = CommonPermissions.ViewContent;
        public static readonly Permission ViewOwnContent = CommonPermissions.ViewOwnContent;
        public static readonly Permission PreviewContent = CommonPermissions.PreviewContent;
        public static readonly Permission PreviewOwnContent = CommonPermissions.PreviewOwnContent;
        public static readonly Permission CloneContent = CommonPermissions.CloneContent;
        public static readonly Permission CloneOwnContent = CommonPermissions.CloneOwnContent;
        public static readonly Permission ListContent = CommonPermissions.ListContent;
        public static readonly Permission EditContentOwner = CommonPermissions.EditContentOwner;

        public static readonly Permission AccessContentApi = new("AccessContentApi", "Access content via the api");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                EditOwnContent,
                EditContent,
                PublishOwnContent,
                PublishContent,
                DeleteOwnContent,
                DeleteContent,
                ViewContent,
                ViewOwnContent,
                PreviewOwnContent,
                PreviewContent,
                CloneContent,
                CloneOwnContent,
                AccessContentApi,
                ListContent,
                EditContentOwner,
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { PublishContent, EditContent, DeleteContent, PreviewContent, CloneContent, AccessContentApi, ListContent, EditContentOwner },
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { PublishContent, EditContent, DeleteContent, PreviewContent, CloneContent, ListContent },
                },
                new PermissionStereotype
                {
                    Name = "Moderator"
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { PublishOwnContent, EditOwnContent, DeleteOwnContent, PreviewOwnContent, CloneOwnContent },
                },
                new PermissionStereotype
                {
                    Name = "Contributor",
                    Permissions = new[] { EditOwnContent, PreviewOwnContent, CloneOwnContent },
                },
                new PermissionStereotype
                {
                    Name = "Authenticated",
                    Permissions = new[] { ViewContent },
                },
                new PermissionStereotype
                {
                    Name = "Anonymous",
                    Permissions = new[] { ViewContent },
                },
            };
        }
    }
}
