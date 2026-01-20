using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Security.Permissions;


namespace OrchardCore.Contents.Security;

/// <summary>
/// The content type permissions helper generates dynamic permissions per content type.
/// </summary>
public static class ContentTypePermissionsHelper
{
    private static readonly Permission _publishContent = new("Publish_{0}", "Publish or unpublish {0} for others", [CommonPermissions.PublishContent]);
    private static readonly Permission _publishOwnContent = new("PublishOwn_{0}", "Publish or unpublish {0}", [_publishContent, CommonPermissions.PublishOwnContent]);
    private static readonly Permission _editContent = new("Edit_{0}", "Edit {0} for others", [_publishContent, CommonPermissions.EditContent]);
    private static readonly Permission _editOwnContent = new("EditOwn_{0}", "Edit {0}", [_editContent, _publishOwnContent, CommonPermissions.EditOwnContent]);
    private static readonly Permission _deleteContent = new("Delete_{0}", "Delete {0} for others", [CommonPermissions.DeleteContent]);
    private static readonly Permission _deleteOwnContent = new("DeleteOwn_{0}", "Delete {0}", [_deleteContent, CommonPermissions.DeleteOwnContent]);
    private static readonly Permission _viewContent = new("View_{0}", "View {0} by others", [_editContent, CommonPermissions.ViewContent]);
    private static readonly Permission _viewOwnContent = new("ViewOwn_{0}", "View own {0}", [_viewContent, CommonPermissions.ViewOwnContent]);
    private static readonly Permission _previewContent = new("Preview_{0}", "Preview {0} by others", [_editContent, CommonPermissions.PreviewContent]);
    private static readonly Permission _previewOwnContent = new("PreviewOwn_{0}", "Preview own {0}", [_previewContent, CommonPermissions.PreviewOwnContent]);
    private static readonly Permission _cloneContent = new("Clone_{0}", "Clone {0} by others", [_editContent, CommonPermissions.CloneContent]);
    private static readonly Permission _cloneOwnContent = new("CloneOwn_{0}", "Clone own {0}", [_cloneContent, CommonPermissions.CloneOwnContent]);
    private static readonly Permission _listContent = new("ListContent_{0}", "List {0} content items", [CommonPermissions.ListContent]);
    private static readonly Permission _editContentOwner = new("EditContentOwner_{0}", "Edit the owner of a {0} content item", [CommonPermissions.EditContentOwner]);

    public static readonly Dictionary<string, Permission> PermissionTemplates = new()
    {
        { CommonPermissions.PublishContent.Name, _publishContent },
        { CommonPermissions.PublishOwnContent.Name, _publishOwnContent },
        { CommonPermissions.EditContent.Name, _editContent },
        { CommonPermissions.EditOwnContent.Name, _editOwnContent },
        { CommonPermissions.DeleteContent.Name, _deleteContent },
        { CommonPermissions.DeleteOwnContent.Name, _deleteOwnContent },
        { CommonPermissions.ViewContent.Name, _viewContent },
        { CommonPermissions.ViewOwnContent.Name, _viewOwnContent },
        { CommonPermissions.PreviewContent.Name, _previewContent },
        { CommonPermissions.PreviewOwnContent.Name, _previewOwnContent },
        { CommonPermissions.CloneContent.Name, _cloneContent },
        { CommonPermissions.CloneOwnContent.Name, _cloneOwnContent },
        { CommonPermissions.ListContent.Name, _listContent },
        { CommonPermissions.EditContentOwner.Name, _editContentOwner },
    };

    private static Dictionary<ValueTuple<string, string>, Permission> _permissionsByType = [];

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

        if (_permissionsByType.TryGetValue(key, out var permission))
        {
            return permission;
        }

        permission = new Permission(
            string.Format(template.Name, contentType),
            string.Format(template.Description, contentType),
            (template.ImpliedBy ?? []).Select(t => CreateDynamicPermission(t, contentType))
        );

        var localPermissions = new Dictionary<ValueTuple<string, string>, Permission>(_permissionsByType)
        {
            [key] = permission,
        };

        _permissionsByType = localPermissions;

        return permission;
    }
}
