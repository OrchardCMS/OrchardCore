using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Autoroute;

public class Permissions : IPermissionProvider
{
    public static readonly Permission SetHomepage = new("SetHomepage", "Set homepage.");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        SetHomepage,
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
