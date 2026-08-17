using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu;

public sealed class Permissions : IPermissionProvider
{

    internal static readonly Permission ListMenuContent = ContentTypePermissionsHelper.CreateDynamicPermission(
        ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.ListContent.Name],
        MenuConstants.MenuContentType);

    private static readonly Permission EditMenuContent = ContentTypePermissionsHelper.CreateDynamicPermission(
        ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditContent.Name],
        MenuConstants.MenuContentType);

    public static readonly Permission ManageMenu = new("ManageMenu", "Manage menus", [EditMenuContent]);

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
