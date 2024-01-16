using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ExecuteGraphQLMutations = CommonPermissions.ExecuteGraphQLMutations;
    public static readonly Permission ExecuteGraphQL = CommonPermissions.ExecuteGraphQL;

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ExecuteGraphQL,
        ExecuteGraphQLMutations,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
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

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
