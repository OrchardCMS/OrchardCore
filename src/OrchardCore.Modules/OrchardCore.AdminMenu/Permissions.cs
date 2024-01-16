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

    private static readonly IEnumerable<Permission> _generalPermissions =
    [
        ManageAdminMenu,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = _generalPermissions,
        },
    ];

    private readonly IAdminMenuService _adminMenuService;

    public Permissions(IAdminMenuService adminMenuService)
    {
        _adminMenuService = adminMenuService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            ViewAdminMenuAll,
            ManageAdminMenu,
        };

        foreach (var adminMenu in (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu)
        {
            permissions.Add(CreatePermissionForAdminMenu(adminMenu.Name));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;

    public static Permission CreatePermissionForAdminMenu(string name)
        => new(
            string.Format(_viewAdminMenu.Name, name),
            string.Format(_viewAdminMenu.Description, name),
            _viewAdminMenu.ImpliedBy
        );
}
