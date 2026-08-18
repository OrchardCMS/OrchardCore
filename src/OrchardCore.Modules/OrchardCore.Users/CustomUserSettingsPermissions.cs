using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public sealed class CustomUserSettingsPermissions : IPermissionProvider
{
    // This permission is never checked it is only used as a template.
    private static readonly PermissionTemplate s_manageOwnCustomUserSettings = new(
        "ManageOwnCustomUserSettings_{0}",
        "Manage Own Custom User Settings - {0}",
        Permissions.ManageUsers);

    private readonly IContentDefinitionManager _contentDefinitionManager;

    public CustomUserSettingsPermissions(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(x => x.GetStereotype() == "CustomUserSettings")
            .Select(CreatePermissionForType);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => [];

    public static Permission CreatePermissionForType(ContentTypeDefinition type) =>
        s_manageOwnCustomUserSettings.CreateDynamicPermission(type.Name, type.DisplayName);
}
