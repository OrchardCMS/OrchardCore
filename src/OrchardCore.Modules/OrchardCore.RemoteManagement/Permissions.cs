using OrchardCore.Security.Permissions;

namespace OrchardCore.RemoteManagement;

public sealed class Permissions : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
        Task.FromResult<IEnumerable<Permission>>(
        [
            RemoteManagementPermissions.AccessRemoteManagement,
            RemoteManagementPermissions.ManageRemoteManagementConfiguration,
        ]);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
        [
            new PermissionStereotype
            {
                Name = OrchardCoreConstants.Roles.Administrator,
                Permissions =
                [
                    RemoteManagementPermissions.AccessRemoteManagement,
                    RemoteManagementPermissions.ManageRemoteManagementConfiguration,
                ],
            },
        ];
}
