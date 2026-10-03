using OrchardCore.Localization;
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

    public static readonly Permission PublishContent = new("PublishContent", LocalizationSource.Create("Publish or unpublish content for others", typeof(CommonPermissions)));

    public static readonly Permission PublishOwnContent = new("PublishOwnContent", LocalizationSource.Create("Publish or unpublish own content", typeof(CommonPermissions)), [PublishContent]);

    public static readonly Permission EditContent = new("EditContent", LocalizationSource.Create("Edit content for others", typeof(CommonPermissions)), [PublishContent]);

    public static readonly Permission EditOwnContent = new("EditOwnContent", LocalizationSource.Create("Edit own content", typeof(CommonPermissions)), [EditContent, PublishOwnContent]);

    public static readonly Permission DeleteContent = new("DeleteContent", LocalizationSource.Create("Delete content for others", typeof(CommonPermissions)));

    public static readonly Permission DeleteOwnContent = new("DeleteOwnContent", LocalizationSource.Create("Delete own content", typeof(CommonPermissions)), [DeleteContent]);

    public static readonly Permission ViewContent = new("ViewContent", LocalizationSource.Create("View all content", typeof(CommonPermissions)), [EditContent]);

    public static readonly Permission ViewOwnContent = new("ViewOwnContent", LocalizationSource.Create("View own content", typeof(CommonPermissions)), [ViewContent]);

    public static readonly Permission PreviewContent = new("PreviewContent", LocalizationSource.Create("Preview content", typeof(CommonPermissions)), [EditContent, PublishContent]);

    public static readonly Permission PreviewOwnContent = new("PreviewOwnContent", LocalizationSource.Create("Preview own content", typeof(CommonPermissions)), [PreviewContent]);

    public static readonly Permission CloneContent = new("CloneContent", LocalizationSource.Create("Clone content", typeof(CommonPermissions)), [EditContent]);

    public static readonly Permission CloneOwnContent = new("CloneOwnContent", LocalizationSource.Create("Clone own content", typeof(CommonPermissions)), [CloneContent]);

    public static readonly Permission ListContent = new("ListContent", LocalizationSource.Create("List content items", typeof(CommonPermissions)));

    public static readonly Permission EditContentOwner = new("EditContentOwner", LocalizationSource.Create("Edit the owner of a content item", typeof(CommonPermissions)));

    public static readonly Permission AccessContentApi = new("AccessContentApi", LocalizationSource.Create("Access content via the api", typeof(CommonPermissions)));

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
