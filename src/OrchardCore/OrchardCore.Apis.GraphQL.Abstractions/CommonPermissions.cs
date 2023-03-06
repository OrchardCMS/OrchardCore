using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public class CommonPermissions
{
    public static readonly Permission ExecuteGraphQLMutations = new("ExecuteGraphQLMutations", "Execute GraphQL Mutations.");
    public static readonly Permission ExecuteGraphQL = new("ExecuteGraphQL", "Execute GraphQL.", new[] { ExecuteGraphQLMutations });
}
