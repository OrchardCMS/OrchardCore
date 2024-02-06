using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public class PermissionsAdminSettings : IPermissionProvider
{
    public static readonly Permission ManageAdminSettings = new("ManageAdminSettings", "Manage Admin Settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageAdminSettings,
    ];

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);
}
