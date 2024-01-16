using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Microsoft.Authentication;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageMicrosoftAuthentication
        = new("ManageMicrosoftAuthentication", "Manage Microsoft Authentication settings");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageMicrosoftAuthentication,
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
