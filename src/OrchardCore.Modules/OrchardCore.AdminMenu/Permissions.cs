using OrchardCore.Admin;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu;

public sealed class Permissions : IPermissionProvider
{
    private static readonly Permission _viewAdminMenu = new("ViewAdminMenu_{0}", "View Admin Menu - {0}", new[] {
        AdminMenuPermissions.ManageAdminMenu,
        AdminMenuPermissions.ViewAdminMenuAll,
    });

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        AdminMenuPermissions.ManageAdminMenu,
    ];

    private readonly IAdminMenuService _adminMenuService;

    [Obsolete("This will be removed in a future release. Instead use 'AdminMenuPermissions.ViewAdminMenuAll'.")]
    public static readonly Permission ManageAdminMenu = AdminMenuPermissions.ViewAdminMenuAll;

    [Obsolete("This will be removed in a future release. Instead use 'AdminMenuPermissions.ManageAdminMenu'.")]
    public static readonly Permission ViewAdminMenuAll = AdminMenuPermissions.ManageAdminMenu;

    public Permissions(IAdminMenuService adminMenuService)
    {
        _adminMenuService = adminMenuService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var adminMenuItems = (await _adminMenuService.GetAdminMenuListAsync().ConfigureAwait(false)).AdminMenu;

        var permissions = new List<Permission>(adminMenuItems.Count + 2)
        {
            AdminMenuPermissions.ViewAdminMenuAll,
            AdminMenuPermissions.ManageAdminMenu,
        };

        foreach (var adminMenu in adminMenuItems)
        {
            permissions.Add(CreatePermissionForAdminMenu(adminMenu.Name));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _generalPermissions,
        },
    ];

    public static Permission CreatePermissionForAdminMenu(string name)
        => new(
            string.Format(_viewAdminMenu.Name, name),
            string.Format(_viewAdminMenu.Description, name),
            _viewAdminMenu.ImpliedBy
        );
}
