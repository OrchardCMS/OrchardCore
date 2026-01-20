using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Microsoft.Authentication;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageMicrosoftAuthentication
        = new("ManageMicrosoftAuthentication", "Manage Microsoft Authentication settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageMicrosoftAuthentication,
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
