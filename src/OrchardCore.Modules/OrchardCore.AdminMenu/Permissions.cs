using OrchardCore.AdminMenu.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageAdminMenu = new("ManageAdminMenu", "Manage the admin menu");
    public static readonly Permission ViewAdminMenuAll = new("ViewAdminMenuAll", "View Admin Menu - View All", new[] { ManageAdminMenu });

    private static readonly Permission _viewAdminMenu = new("ViewAdminMenu_{0}", "View Admin Menu - {0}", new[] { ManageAdminMenu, ViewAdminMenuAll });

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        ManageAdminMenu,
    ];

    private readonly IAdminMenuService _adminMenuService;

    public Permissions(IAdminMenuService adminMenuService)
    {
        _adminMenuService = adminMenuService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var adminMenuItems = (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu;

        var permissions = new List<Permission>(adminMenuItems.Count + 2)
        {
            ViewAdminMenuAll,
            ManageAdminMenu,
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
