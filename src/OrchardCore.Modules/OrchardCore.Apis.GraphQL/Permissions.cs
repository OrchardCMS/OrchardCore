using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ExecuteGraphQLMutations = CommonPermissions.ExecuteGraphQLMutations;
    public static readonly Permission ExecuteGraphQL = CommonPermissions.ExecuteGraphQL;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ExecuteGraphQL,
        ExecuteGraphQLMutations,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions =
            [
                ExecuteGraphQLMutations,
            ],
        },
    ];
}
