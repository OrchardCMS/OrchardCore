using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public sealed class Permissions : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Apis.GraphQL.CommonPermissions.ExecuteGraphQLMutations'.")]
    public static readonly Permission ExecuteGraphQLMutations = CommonPermissions.ExecuteGraphQLMutations;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Apis.GraphQL.CommonPermissions.ExecuteGraphQL'.")]
    public static readonly Permission ExecuteGraphQL = CommonPermissions.ExecuteGraphQL;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        CommonPermissions.ExecuteGraphQL,
        CommonPermissions.ExecuteGraphQLMutations,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions =
            [
                CommonPermissions.ExecuteGraphQLMutations,
            ],
        },
    ];
}
