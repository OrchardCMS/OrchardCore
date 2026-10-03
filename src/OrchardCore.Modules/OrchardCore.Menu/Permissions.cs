using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu;

public sealed class Permissions : IPermissionProvider
{
    private static readonly Permission s_editMenuContent = ContentTypePermissionsHelper.CreateDynamicPermission(
        ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditContent.Name],
        "Menu");

    public static readonly Permission ManageMenu = new("ManageMenu", LocalizationSource.Create<Permissions>("Manage menus"), [s_editMenuContent]);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageMenu,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
