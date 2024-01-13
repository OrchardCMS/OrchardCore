using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageAdminMenu = new("ManageAdminMenu", "Manage the admin menu");

    public static readonly Permission ViewAdminMenuAll = new("ViewAdminMenuAll", "View Admin Menu - View All", new[] { ManageAdminMenu });

    private static readonly Permission _viewAdminMenu = new("ViewAdminMenu_{0}", "View Admin Menu - {0}", new[] { ManageAdminMenu, ViewAdminMenuAll });

    private readonly IAdminMenuService _adminMenuService;

    public Permissions(IAdminMenuService adminMenuService)
    {
        _adminMenuService = adminMenuService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var list = new List<Permission>(_allPermissions);

        foreach (var adminMenu in (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu)
        {
            list.Add(CreatePermissionForAdminMenu(adminMenu.Name));
        }

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    public static Permission CreatePermissionForAdminMenu(string name)
        => new(
                string.Format(_viewAdminMenu.Name, name),
                string.Format(_viewAdminMenu.Description, name),
                _viewAdminMenu.ImpliedBy
            );

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = _generalPermissions,
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ViewAdminMenuAll,
        ManageAdminMenu,
    ];

    private readonly static IEnumerable<Permission> _generalPermissions =
    [
        ManageAdminMenu,
    ];
}
