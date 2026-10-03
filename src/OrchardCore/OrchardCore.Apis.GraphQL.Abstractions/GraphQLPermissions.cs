using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public static class GraphQLPermissions
{
    public static readonly Permission ApiViewContent = new("ApiViewContent", new LocalizationSource("Access view content endpoints", typeof(GraphQLPermissions)));

    public static readonly Permission ExecuteGraphQLMutations = new("ExecuteGraphQLMutations", new LocalizationSource("Execute GraphQL Mutations.", typeof(GraphQLPermissions)));

    public static readonly Permission ExecuteGraphQL = new("ExecuteGraphQL", new LocalizationSource("Execute GraphQL.", typeof(GraphQLPermissions)), [ExecuteGraphQLMutations]);
}
