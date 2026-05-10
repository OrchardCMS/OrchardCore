using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenSearch;

public sealed class PermissionProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        OpenSearchPermissions.ManageOpenSearchIndexes,
        OpenSearchPermissions.QueryOpenSearchApi,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
        Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions =
            [
                OpenSearchPermissions.ManageOpenSearchIndexes,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                OpenSearchPermissions.QueryOpenSearchApi,
            ],
        },
    ];
}
