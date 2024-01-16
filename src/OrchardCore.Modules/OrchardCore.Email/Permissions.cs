using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageEmailSettings = new("ManageEmailSettings", "Manage Email Settings");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageEmailSettings,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
