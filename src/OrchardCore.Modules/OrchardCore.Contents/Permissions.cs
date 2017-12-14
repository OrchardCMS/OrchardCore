using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents
{
    public class Permissions : IPermissionProvider
    {

        // Note - in code you should demand PublishContent, EditContent, or DeleteContent
        // Do not demand the "Own" variations - those are applied automatically when you demand the main ones

        public static readonly Permission PublishContent = new Permission("PublishContent", "Publish or unpublish content for others");
        public static readonly Permission PublishOwnContent = new Permission("PublishOwnContent", "Publish or unpublish own content", new[] { PublishContent });
        public static readonly Permission EditContent = new Permission("EditContent", "Edit content for others", new[] { PublishContent });
        public static readonly Permission EditOwnContent = new Permission("EditOwnContent", "Edit own content", new[] { EditContent, PublishOwnContent });
        public static readonly Permission DeleteContent = new Permission("DeleteContent", "Delete content for others");
        public static readonly Permission DeleteOwnContent = new Permission("DeleteOwnContent", "Delete own content", new[] { DeleteContent });
        public static readonly Permission ViewContent = new Permission("ViewContent", "View all content", new[] { EditContent });
        public static readonly Permission ViewOwnContent = new Permission("ViewOwnContent", "View own content", new[] { ViewContent });
        public static readonly Permission PreviewContent = new Permission("PreviewContent", "Preview content", new[] { EditContent, PublishContent });
        public static readonly Permission PreviewOwnContent = new Permission("PreviewOwnContent", "Preview own content", new[] { PreviewContent });


        //public static readonly Permission MetaListContent = new Permission { ImpliedBy = new[] { EditOwnContent, PublishOwnContent, DeleteOwnContent } };

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                EditOwnContent,
                EditContent,
                PublishOwnContent,
                PublishContent,
                DeleteOwnContent,
                DeleteContent,
                ViewContent,
                ViewOwnContent,
                PreviewOwnContent,
                PreviewContent
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {PublishContent,EditContent,DeleteContent,PreviewContent}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {PublishContent,EditContent,DeleteContent,PreviewContent}
                },
                new PermissionStereotype {
                    Name = "Moderator"
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {PublishOwnContent,EditOwnContent,DeleteOwnContent,PreviewOwnContent}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {EditOwnContent,PreviewOwnContent}
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {ViewContent}
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = new[] {ViewContent}
                },
            };
        }

    }
}