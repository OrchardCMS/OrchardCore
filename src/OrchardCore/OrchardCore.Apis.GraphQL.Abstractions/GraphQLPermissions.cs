using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public static class GraphQLPermissions
{
    public static readonly Permission ApiViewContent = new("ApiViewContent", LocalizationSource.Create("Access view content endpoints", typeof(GraphQLPermissions)));

    public static readonly Permission ExecuteGraphQLMutations = new("ExecuteGraphQLMutations", LocalizationSource.Create("Execute GraphQL Mutations.", typeof(GraphQLPermissions)));

    public static readonly Permission ExecuteGraphQL = new("ExecuteGraphQL", LocalizationSource.Create("Execute GraphQL.", typeof(GraphQLPermissions)), [ExecuteGraphQLMutations]);
}
