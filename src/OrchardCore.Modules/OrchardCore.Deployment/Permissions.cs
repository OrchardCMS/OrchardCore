using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public class Permissions : IPermissionProvider
{
    [Obsolete("This property will be removed in future release. Instead use 'CommonPermissions.ManageDeploymentPlan'.")]
    public static readonly Permission ManageDeploymentPlan = CommonPermissions.ManageDeploymentPlan;

    [Obsolete("This property will be removed in future release. Instead use 'CommonPermissions.Export'.")]
    public static readonly Permission Export = CommonPermissions.Export;

    [Obsolete("This property will be removed in future release. Instead use 'CommonPermissions.Import'.")]
    public static readonly Permission Import = CommonPermissions.Import;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        CommonPermissions.Import,
        CommonPermissions.Export,
        CommonPermissions.ManageDeploymentPlan,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
