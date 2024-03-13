using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public class SecurityPermissions : IPermissionProvider
{
    public static readonly Permission ManageSecurityHeadersSettings = new("ManageSecurityHeadersSettings", "Manage Security Headers Settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSecurityHeadersSettings,
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
