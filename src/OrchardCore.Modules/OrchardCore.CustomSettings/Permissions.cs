using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.CustomSettings;

public sealed class Permissions : IPermissionProvider
{
    private static readonly PermissionTemplate s_manageCustomSettings = new(
        "ManageCustomSettings_{0}",
        "Manage Custom Settings - {0}",
        new Permission("ManageResourceSettings"));

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
        => string.Format(s_manageCustomSettings.Name, name);

    public static Permission CreatePermissionForType(ContentTypeDefinition type)
        => s_manageCustomSettings.CreateDynamicPermission(type.Name, type.DisplayName);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => [];
}
