using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Remote;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageRemoteInstances = new("ManageRemoteInstances", "Manage remote instances");
    public static readonly Permission ManageRemoteClients = new("ManageRemoteClients", "Manage remote clients");
    public static readonly Permission Export = new("ExportRemoteInstances", "Export to remote instances");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ManageRemoteInstances,
        ManageRemoteClients,
        Export,
    ];
}
