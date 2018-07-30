using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Mutations;
using OrchardCore.ContentManagement.GraphQL.Mutations.Types;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Providers;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContentGraphQL(this IServiceCollection services)
        {
            services.AddGraphMutationType<CreateContentItemMutation>();
            services.AddGraphMutationType<DeleteContentItemMutation>();

            services.AddGraphQueryType<ContentItemQuery>();
            services.AddGraphQueryType<ContentItemsQuery>();
            services.AddScoped<ContentItemType>();
            services.AddScoped<DeletionStatusObjectGraphType>();
            services.AddScoped<CreateContentItemInputType>();
            services.AddScoped<ContentPartsInputType>();

            services.AddScoped<IQueryFieldTypeProvider, ContentItemFieldTypeProvider>();

            services.AddScoped<IGraphQLSchemaHashService, GraphQLSchemaHashService>();

            services.AddScoped<IPermissionProvider, Permissions>();

            return services;
        }
    }
}
