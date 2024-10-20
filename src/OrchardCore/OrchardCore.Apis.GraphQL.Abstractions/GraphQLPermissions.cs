using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public static class GraphQLPermissions
{
    public static readonly Permission ApiViewContent = new("ApiViewContent", "Access view content endpoints");

    public static readonly Permission ExecuteGraphQLMutations = new("ExecuteGraphQLMutations", "Execute GraphQL Mutations.");

    public static readonly Permission ExecuteGraphQL = new("ExecuteGraphQL", "Execute GraphQL.", [ExecuteGraphQLMutations]);
}
