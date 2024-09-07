using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public sealed class CustomUserSettingsPermissions : IPermissionProvider
{
    // This permission is never checked it is only used as a template.
    private static readonly Permission _manageOwnCustomUserSettings = new("ManageOwnCustomUserSettings_{0}", "Manage Own Custom User Settings - {0}", new[] { Permissions.ManageUsers });

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
        new(
            string.Format(_manageOwnCustomUserSettings.Name, type.Name),
            string.Format(_manageOwnCustomUserSettings.Description, type.DisplayName),
            _manageOwnCustomUserSettings.ImpliedBy
        );
}
