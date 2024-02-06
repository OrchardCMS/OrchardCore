using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageMenu = new("ManageMenu", "Manage menus");

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
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];
}
