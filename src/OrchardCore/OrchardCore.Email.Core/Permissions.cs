using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageEmailSettings = new("ManageEmailSettings", "Manage Email Settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageEmailSettings,
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
