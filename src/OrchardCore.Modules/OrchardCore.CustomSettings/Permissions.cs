using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.CustomSettings;

public sealed class Permissions : IPermissionProvider
{
    private static readonly Permission _manageCustomSettings = new("ManageCustomSettings_{0}", "Manage Custom Settings - {0}", new[] { new Permission("ManageResourceSettings") });

    private readonly CustomSettingsService _customSettingsService;

    public Permissions(CustomSettingsService customSettingsService)
    {
        _customSettingsService = customSettingsService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var list = new List<Permission>();

        foreach (var type in await _customSettingsService.GetAllSettingsTypesAsync())
        {
            list.Add(CreatePermissionForType(type));
        }

        return list;
    }

    public static string CreatePermissionName(string name)
        => string.Format(_manageCustomSettings.Name, name);

    public static Permission CreatePermissionForType(ContentTypeDefinition type)
    {
        return new Permission(
                CreatePermissionName(type.Name),
                string.Format(_manageCustomSettings.Description, type.DisplayName),
                _manageCustomSettings.ImpliedBy
            );
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => [];
}
