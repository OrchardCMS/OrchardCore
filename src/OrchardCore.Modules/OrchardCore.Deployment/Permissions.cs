using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageDeploymentPlan = CommonPermissions.ManageDeploymentPlan;
    public static readonly Permission Export = CommonPermissions.Export;
    public static readonly Permission Import = CommonPermissions.Import;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        Import,
        Export,
        ManageDeploymentPlan,
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
