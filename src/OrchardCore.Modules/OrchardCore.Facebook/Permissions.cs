using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageFacebookApp = new("ManageFacebookApp", "View and edit the Facebook app.");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageFacebookApp,
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
