using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL;

public static class GraphQLPermissions
{
    public static readonly Permission ApiViewContent = new("ApiViewContent", LocalizedString.Create("Access view content endpoints", typeof(GraphQLPermissions)));

    public static readonly Permission ExecuteGraphQLMutations = new("ExecuteGraphQLMutations", LocalizedString.Create("Execute GraphQL Mutations.", typeof(GraphQLPermissions)));

    public static readonly Permission ExecuteGraphQL = new("ExecuteGraphQL", LocalizedString.Create("Execute GraphQL.", typeof(GraphQLPermissions)), [ExecuteGraphQLMutations]);
}
