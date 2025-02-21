using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        GraphQLPermissions.ExecuteGraphQL,
        GraphQLPermissions.ExecuteGraphQLMutations,
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
                GraphQLPermissions.ExecuteGraphQLMutations,
            ],
        },
    ];
}
