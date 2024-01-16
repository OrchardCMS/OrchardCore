using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public class SecurityPermissions : IPermissionProvider
{
    public static readonly Permission ManageSecurityHeadersSettings = new("ManageSecurityHeadersSettings", "Manage Security Headers Settings");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSecurityHeadersSettings,
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
