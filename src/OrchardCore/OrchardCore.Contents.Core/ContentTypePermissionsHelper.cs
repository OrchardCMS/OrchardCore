using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Security.Permissions;


namespace OrchardCore.Contents.Security;

/// <summary>
/// The content type permissions helper generates dynamic permissions per content type.
/// </summary>
public static class ContentTypePermissionsHelper
{
    private static readonly Permission s_publishContent = new("Publish_{0}", "Publish or unpublish {0} for others", [CommonPermissions.PublishContent]);
    private static readonly Permission s_publishOwnContent = new("PublishOwn_{0}", "Publish or unpublish {0}", [s_publishContent, CommonPermissions.PublishOwnContent]);
    private static readonly Permission s_editContent = new("Edit_{0}", "Edit {0} for others", [s_publishContent, CommonPermissions.EditContent]);
    private static readonly Permission s_editOwnContent = new("EditOwn_{0}", "Edit {0}", [s_editContent, s_publishOwnContent, CommonPermissions.EditOwnContent]);
    private static readonly Permission s_deleteContent = new("Delete_{0}", "Delete {0} for others", [CommonPermissions.DeleteContent]);
    private static readonly Permission s_deleteOwnContent = new("DeleteOwn_{0}", "Delete {0}", [s_deleteContent, CommonPermissions.DeleteOwnContent]);
    private static readonly Permission s_viewContent = new("View_{0}", "View {0} by others", [s_editContent, CommonPermissions.ViewContent]);
    private static readonly Permission s_viewOwnContent = new("ViewOwn_{0}", "View own {0}", [s_viewContent, CommonPermissions.ViewOwnContent]);
    private static readonly Permission s_previewContent = new("Preview_{0}", "Preview {0} by others", [s_editContent, CommonPermissions.PreviewContent]);
    private static readonly Permission s_previewOwnContent = new("PreviewOwn_{0}", "Preview own {0}", [s_previewContent, CommonPermissions.PreviewOwnContent]);
    private static readonly Permission s_cloneContent = new("Clone_{0}", "Clone {0} by others", [s_editContent, CommonPermissions.CloneContent]);
    private static readonly Permission s_cloneOwnContent = new("CloneOwn_{0}", "Clone own {0}", [s_cloneContent, CommonPermissions.CloneOwnContent]);
    private static readonly Permission s_listContent = new("ListContent_{0}", "List {0} content items", [CommonPermissions.ListContent]);
    private static readonly Permission s_editContentOwner = new("EditContentOwner_{0}", "Edit the owner of a {0} content item", [CommonPermissions.EditContentOwner]);

    public static readonly Dictionary<string, Permission> PermissionTemplates = new()
    {
        { CommonPermissions.PublishContent.Name, s_publishContent },
        { CommonPermissions.PublishOwnContent.Name, s_publishOwnContent },
        { CommonPermissions.EditContent.Name, s_editContent },
        { CommonPermissions.EditOwnContent.Name, s_editOwnContent },
        { CommonPermissions.DeleteContent.Name, s_deleteContent },
        { CommonPermissions.DeleteOwnContent.Name, s_deleteOwnContent },
        { CommonPermissions.ViewContent.Name, s_viewContent },
        { CommonPermissions.ViewOwnContent.Name, s_viewOwnContent },
        { CommonPermissions.PreviewContent.Name, s_previewContent },
        { CommonPermissions.PreviewOwnContent.Name, s_previewOwnContent },
        { CommonPermissions.CloneContent.Name, s_cloneContent },
        { CommonPermissions.CloneOwnContent.Name, s_cloneOwnContent },
        { CommonPermissions.ListContent.Name, s_listContent },
        { CommonPermissions.EditContentOwner.Name, s_editContentOwner },
    };

    private static Dictionary<ValueTuple<string, string>, Permission> s_permissionsByType = [];

    /// <summary>
    /// Returns a dynamic permission for a content type, based on a global content permission template.
    /// </summary>
    public static Permission ConvertToDynamicPermission(Permission permission)
    {
        if (PermissionTemplates.TryGetValue(permission.Name, out var result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// Generates a permission dynamically for a content type.
    /// </summary>
    public static Permission CreateDynamicPermission(Permission template, ContentTypeDefinition typeDefinition)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new Permission(
            string.Format(template.Name, typeDefinition.Name),
            string.Format(template.Description, typeDefinition.DisplayName),
            (template.ImpliedBy ?? [])
            .Where(t => t != null)
            .Select(t => CreateDynamicPermission(t, typeDefinition))
        )
        {
            Category = $"{typeDefinition.DisplayName} Content Type - {typeDefinition.Name}",
        };
    }

    /// <summary>
    /// Generates a permission dynamically for a content type, without a display name or category.
    /// </summary>
    public static Permission CreateDynamicPermission(Permission template, string contentType)
    {
        ArgumentNullException.ThrowIfNull(template);

        var key = new ValueTuple<string, string>(template.Name, contentType);

        if (s_permissionsByType.TryGetValue(key, out var permission))
        {
            return permission;
        }

        permission = new Permission(
            string.Format(template.Name, contentType),
            string.Format(template.Description, contentType),
            (template.ImpliedBy ?? []).Select(t => CreateDynamicPermission(t, contentType))
        );

        var localPermissions = new Dictionary<ValueTuple<string, string>, Permission>(s_permissionsByType)
        {
            [key] = permission,
        };

        s_permissionsByType = localPermissions;

        return permission;
    }
}
