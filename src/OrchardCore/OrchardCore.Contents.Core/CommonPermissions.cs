using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents;

/// <summary>
/// This class contains the source references to the OrchardCore.Contents module permissions so they can be used in other modules
/// without having to reference the OrchardCore.Contents by itself.
/// </summary>
public static class CommonPermissions
{
    // Note - in code you should demand PublishContent, EditContent, or DeleteContent
    // Do not demand the "Own" variations - those are applied automatically when you demand the main ones

    // EditOwn is the permission that is ultimately required to create new content. See how the Create() method is implemented in the AdminController

    public static readonly Permission PublishContent = new("PublishContent", "Publish or unpublish content for others");

    public static readonly Permission PublishOwnContent = new("PublishOwnContent", "Publish or unpublish own content", [PublishContent]);

    public static readonly Permission EditContent = new("EditContent", "Edit content for others", [PublishContent]);

    public static readonly Permission EditOwnContent = new("EditOwnContent", "Edit own content", [EditContent, PublishOwnContent]);

    public static readonly Permission DeleteContent = new("DeleteContent", "Delete content for others");

    public static readonly Permission DeleteOwnContent = new("DeleteOwnContent", "Delete own content", [DeleteContent]);

    public static readonly Permission ViewContent = new("ViewContent", "View all content", [EditContent]);

    public static readonly Permission ViewOwnContent = new("ViewOwnContent", "View own content", [ViewContent]);

    public static readonly Permission PreviewContent = new("PreviewContent", "Preview content", [EditContent, PublishContent]);

    public static readonly Permission PreviewOwnContent = new("PreviewOwnContent", "Preview own content", [PreviewContent]);

    public static readonly Permission CloneContent = new("CloneContent", "Clone content", [EditContent]);

    public static readonly Permission CloneOwnContent = new("CloneOwnContent", "Clone own content", [CloneContent]);

    public static readonly Permission ListContent = new("ListContent", "List content items");

    public static readonly Permission EditContentOwner = new("EditContentOwner", "Edit the owner of a content item");

    public static readonly Permission AccessContentApi = new("AccessContentApi", "Access content via the api");

    public static readonly Dictionary<string, Permission> OwnerPermissionsByName = [];

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
