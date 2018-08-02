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
            services.AddTransient<ContentItemType>();
            services.AddTransient<DeletionStatusObjectGraphType>();
            services.AddTransient<CreateContentItemInputType>();
            services.AddTransient<ContentPartsInputType>();

            services.AddTransient<IQueryFieldTypeProvider, ContentItemFieldTypeProvider>();

            services.AddScoped<IGraphQLSchemaHashService, GraphQLSchemaHashService>();

            services.AddScoped<IPermissionProvider, Permissions>();

            return services;
        }
    }
}
