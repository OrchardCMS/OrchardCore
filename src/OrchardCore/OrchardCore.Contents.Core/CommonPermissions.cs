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
        public static readonly Permission CloneContent = new Permission("CloneContent", "Clone content", new[] { EditContent });
        public static readonly Permission CloneOwnContent = new Permission("CloneOwnContent", "Clone own content", new[] { CloneContent });
    }
}
