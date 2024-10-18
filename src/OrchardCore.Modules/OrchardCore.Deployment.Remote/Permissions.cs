using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Remote;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        DeploymentPermissions.ManageRemoteInstances,
        DeploymentPermissions.ManageRemoteClients,
        DeploymentPermissions.ExportData,
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
