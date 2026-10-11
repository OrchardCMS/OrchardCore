using OrchardCore.Security.Permissions;

namespace OrchardCore.DataPipelines;

public sealed class PermissionsProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        DataPipelinePermissions.ManageDataPipelines,
        DataPipelinePermissions.RunDataPipelines,
        DataPipelinePermissions.ViewDataPipelines,
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
