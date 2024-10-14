using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Remote;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageRemoteInstances = new("ManageRemoteInstances", "Manage remote instances");
    public static readonly Permission ManageRemoteClients = new("ManageRemoteClients", "Manage remote clients");
    public static readonly Permission Export = new("ExportRemoteInstances", "Export to remote instances");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageRemoteInstances,
        ManageRemoteClients,
        Export,
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
