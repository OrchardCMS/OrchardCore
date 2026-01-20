using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Https;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageHttps = new("ManageHttps", "Manage HTTPS");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageHttps,
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
