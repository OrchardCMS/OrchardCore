using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public sealed class PermissionProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        ElasticsearchPermissions.ManageElasticIndexes,
        ElasticsearchPermissions.QueryElasticApi,
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
                ElasticsearchPermissions.ManageElasticIndexes,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                ElasticsearchPermissions.QueryElasticApi,
            ],
        },
    ];
}
