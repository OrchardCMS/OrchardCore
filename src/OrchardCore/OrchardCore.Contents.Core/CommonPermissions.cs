using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents
{
    /// <summary>
    /// This class contains the source references to the OrchardCore.Contents module permissions so they can be used in other modules
    /// without having to reference the OrchardCore.Contents by itself.
    /// </summary>
    public class CommonPermissions
    {
        // Note - in code you should demand PublishContent, EditContent, or DeleteContent
        // Do not demand the "Own" variations - those are applied automatically when you demand the main ones

        // EditOwn is the permission that is ultimately required to create new content. See how the Create() method is implemented in the AdminController

        public static readonly Permission PublishContent = new("PublishContent", "Publish or unpublish content for others");
        public static readonly Permission PublishOwnContent = new("PublishOwnContent", "Publish or unpublish own content", new[] { PublishContent });
        public static readonly Permission EditContent = new("EditContent", "Edit content for others", new[] { PublishContent });
        public static readonly Permission EditOwnContent = new("EditOwnContent", "Edit own content", new[] { EditContent, PublishOwnContent });
        public static readonly Permission DeleteContent = new("DeleteContent", "Delete content for others");
        public static readonly Permission DeleteOwnContent = new("DeleteOwnContent", "Delete own content", new[] { DeleteContent });
        public static readonly Permission ViewContent = new("ViewContent", "View all content", new[] { EditContent });
        public static readonly Permission ViewOwnContent = new("ViewOwnContent", "View own content", new[] { ViewContent });
        public static readonly Permission PreviewContent = new("PreviewContent", "Preview content", new[] { EditContent, PublishContent });
        public static readonly Permission PreviewOwnContent = new("PreviewOwnContent", "Preview own content", new[] { PreviewContent });
        public static readonly Permission CloneContent = new("CloneContent", "Clone content", new[] { EditContent });
        public static readonly Permission CloneOwnContent = new("CloneOwnContent", "Clone own content", new[] { CloneContent });
        public static readonly Permission ListContent = new("ListContent", "List content items");
        public static readonly Permission EditContentOwner = new("EditContentOwner", "Edit the owner of a content item");

        public static readonly Dictionary<string, Permission> OwnerPermissionsByName = new();

        static CommonPermissions()
        {
            OwnerPermissionsByName["PublishContent"] = PublishOwnContent;
            OwnerPermissionsByName["EditContent"] = EditOwnContent;
            OwnerPermissionsByName["DeleteContent"] = DeleteOwnContent;
            OwnerPermissionsByName["ViewContent"] = ViewOwnContent;
            OwnerPermissionsByName["PreviewContent"] = PreviewOwnContent;
            OwnerPermissionsByName["CloneContent"] = CloneOwnContent;
        }
    }
}
