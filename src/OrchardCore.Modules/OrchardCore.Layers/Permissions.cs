using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Layers;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageLayers = new("ManageLayers", "Manage layers");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageLayers,
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
