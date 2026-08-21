using OrchardCore.Security.Permissions;

namespace OrchardCore.DataOrchestrator;

public sealed class PermissionsProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        EtlPermissions.ManageEtlPipelines,
        EtlPermissions.ExecuteEtlPipelines,
        EtlPermissions.ViewEtlPipelines,
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
