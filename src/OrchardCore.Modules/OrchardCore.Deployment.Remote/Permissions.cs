using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Remote;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        DeploymentPermissions.ManageRemoteInstances,
        DeploymentPermissions.ManageRemoteClients,
        DeploymentPermissions.ExportRemoteInstances,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'DeploymentPermissions.ManageRemoteInstances'.")]
    public static readonly Permission ManageRemoteInstances = new("ManageRemoteInstances", "Manage remote instances");

    [Obsolete("This will be removed in a future release. Instead use 'DeploymentPermissions.ManageRemoteClients'.")]
    public static readonly Permission ManageRemoteClients = new("ManageRemoteClients", "Manage remote clients");

    [Obsolete("This will be removed in a future release. Instead use 'DeploymentPermissions.ExportRemoteInstances'.")]
    public static readonly Permission Export = new("ExportRemoteInstances", "Export to remote instances");

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
