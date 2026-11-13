using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Localization;
using OrchardCore.Security.Permissions;


namespace OrchardCore.Contents.Security;

/// <summary>
/// The content type permissions helper generates dynamic permissions per content type.
/// </summary>
public static class ContentTypePermissionsHelper
{
    private static readonly PermissionTemplate s_publishContent = CreateTemplate("Publish", LocalizationSource.Create("Publish or unpublish {0} for others", typeof(ContentTypePermissionsHelper)), CommonPermissions.PublishContent);
    private static readonly PermissionTemplate s_publishOwnContent = CreateTemplate("PublishOwn", LocalizationSource.Create("Publish or unpublish {0}", typeof(ContentTypePermissionsHelper)), CommonPermissions.PublishOwnContent, s_publishContent);
    private static readonly PermissionTemplate s_editContent = CreateTemplate("Edit", LocalizationSource.Create("Edit {0} for others", typeof(ContentTypePermissionsHelper)), CommonPermissions.EditContent, s_publishContent);
    private static readonly PermissionTemplate s_editOwnContent = CreateTemplate("EditOwn", LocalizationSource.Create("Edit {0}", typeof(ContentTypePermissionsHelper)), CommonPermissions.EditOwnContent, s_editContent, s_publishOwnContent);
    private static readonly PermissionTemplate s_deleteContent = CreateTemplate("Delete", LocalizationSource.Create("Delete {0} for others", typeof(ContentTypePermissionsHelper)), CommonPermissions.DeleteContent);
    private static readonly PermissionTemplate s_deleteOwnContent = CreateTemplate("DeleteOwn", LocalizationSource.Create("Delete {0}", typeof(ContentTypePermissionsHelper)), CommonPermissions.DeleteOwnContent, s_deleteContent);
    private static readonly PermissionTemplate s_viewContent = CreateTemplate("View", LocalizationSource.Create("View {0} by others", typeof(ContentTypePermissionsHelper)), CommonPermissions.ViewContent, s_editContent);
    private static readonly PermissionTemplate s_viewOwnContent = CreateTemplate("ViewOwn", LocalizationSource.Create("View own {0}", typeof(ContentTypePermissionsHelper)), CommonPermissions.ViewOwnContent, s_viewContent);
    private static readonly PermissionTemplate s_previewContent = CreateTemplate("Preview", LocalizationSource.Create("Preview {0} by others", typeof(ContentTypePermissionsHelper)), CommonPermissions.PreviewContent, s_editContent);
    private static readonly PermissionTemplate s_previewOwnContent = CreateTemplate("PreviewOwn", LocalizationSource.Create("Preview own {0}", typeof(ContentTypePermissionsHelper)), CommonPermissions.PreviewOwnContent, s_previewContent);
    private static readonly PermissionTemplate s_cloneContent = CreateTemplate("Clone", LocalizationSource.Create("Clone {0} by others", typeof(ContentTypePermissionsHelper)), CommonPermissions.CloneContent, s_editContent);
    private static readonly PermissionTemplate s_cloneOwnContent = CreateTemplate("CloneOwn", LocalizationSource.Create("Clone own {0}", typeof(ContentTypePermissionsHelper)), CommonPermissions.CloneOwnContent, s_cloneContent);
    private static readonly PermissionTemplate s_listContent = CreateTemplate("ListContent", LocalizationSource.Create("List {0} content items", typeof(ContentTypePermissionsHelper)), CommonPermissions.ListContent);
    private static readonly PermissionTemplate s_editContentOwner = CreateTemplate("EditContentOwner", LocalizationSource.Create("Edit the owner of a {0} content item", typeof(ContentTypePermissionsHelper)), CommonPermissions.EditContentOwner);

    public static readonly Dictionary<string, PermissionTemplate> PermissionTemplates = new()
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
    [Obsolete($"Use {nameof(GetPermissionTemplate)} instead.")]
    public static Permission ConvertToDynamicPermission(Permission permission) =>
        PermissionTemplates.TryGetValue(permission.Name, out var result) ? result.CreateDynamicPermission("{0}") : null;

    /// <summary>
    /// Generates a permission dynamically for a content type.
    /// </summary>
    [Obsolete($"Use {nameof(CreateDynamicPermissionOf)} instead.")]
    public static Permission CreateDynamicPermission(Permission template, ContentTypeDefinition typeDefinition)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new Permission(
            string.Format(template.Name, typeDefinition.Name),
            string.Format(template.Description.Value, typeDefinition.DisplayName),
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
    [Obsolete($"Use {nameof(CreateDynamicPermissionOf)} instead.")]
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
            string.Format(template.Description.Value, contentType),
            (template.ImpliedBy ?? []).Select(t => CreateDynamicPermission(t, contentType))
        );

        var localPermissions = new Dictionary<ValueTuple<string, string>, Permission>(s_permissionsByType)
        {
            [key] = permission,
        };

        s_permissionsByType = localPermissions;

        return permission;
    }

    /// <summary>
    /// Returns a permission template for content types, based on a global content permission with the name of <paramref
    /// name="basePermissionName"/>.
    /// </summary>
    public static PermissionTemplate GetPermissionTemplate(string basePermissionName) =>
        PermissionTemplates.TryGetValue(basePermissionName, out var result) ? result : null;

    /// <summary>
    /// Generates a dynamic version of the provided <paramref name="basePermission"/>, that is specific to content type
    /// indicated by the <paramref name="typeDefinition"/>.
    /// </summary>
    public static Permission CreateDynamicPermissionOf(Permission basePermission, ContentTypeDefinition typeDefinition) =>
        CreateDynamicPermissionOf(basePermission.Name, typeDefinition);

    /// <summary>
    /// Generates a permission dynamically for a content type, without a display name or category.
    /// </summary>
    public static Permission CreateDynamicPermissionOf(string basePermissionName, string contentType) =>
        CreateDynamicPermissionOf(basePermissionName, contentType, contentType);

    /// <summary>
    /// Generates a dynamic version of the provided permission with the name <paramref name="basePermissionName"/>,
    /// that is specific to a content type indicated by the <paramref name="typeDefinition"/>.
    /// </summary>
    public static Permission CreateDynamicPermissionOf(string basePermissionName, ContentTypeDefinition typeDefinition) =>
        CreateDynamicPermissionOf(basePermissionName, typeDefinition.Name, typeDefinition.DisplayName);
    
    /// <summary>
    /// Generates a dynamic version of the provided permission with the name <paramref name="basePermissionName"/>,
    /// that is specific to a <paramref name="contentType"/>.
    /// </summary>
    private static Permission CreateDynamicPermissionOf(string basePermissionName, string contentType, string contentTypeDisplayName)
    {
        if (GetPermissionTemplate(basePermissionName) is not { } template)
        {
            return null;
        }

        return template.CreateDynamicPermission(contentType, contentTypeDisplayName);
    }

    private static PermissionTemplate CreateTemplate(
        string nameBase,
        LocalizationSource description,
        Permission impliedBy,
        params PermissionTemplate[] impliedByTemplates) =>
        new(
            nameBase + "_{0}",
            description,
            "{1} Content Type - {0}",
            impliedBy)
        {
            ImpliedByTemplates = impliedByTemplates,
        };
}
