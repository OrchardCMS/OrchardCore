using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public sealed class Permissions : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan'.")]
    public static readonly Permission ManageDeploymentPlan = CommonPermissions.ManageDeploymentPlan;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Deployment.CommonPermissions.Export'.")]
    public static readonly Permission Export = CommonPermissions.Export;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Deployment.CommonPermissions.Import'.")]
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
