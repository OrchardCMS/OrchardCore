using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Security.Permissions;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContentGraphQL(this IServiceCollection services)
        {
            services.AddSingleton<ISchemaBuilder, ContentItemQuery>();
            services.AddSingleton<ISchemaBuilder, ContentTypeQuery>();
            services.AddSingleton<ContentItemInterface>();

            services.AddTransient<ContentItemType>();

            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddTransient<DynamicPartGraphType>();
            services.AddScoped<IContentTypeBuilder, TypedContentTypeBuilder>();
            services.AddScoped<IContentTypeBuilder, DynamicContentTypeBuilder>();

            services.AddOptions<GraphQLContentOptions>();
            services.AddGraphQLFilterType<ContentItem, ContentItemFilters>();
            services.AddWhereInputIndexPropertyProvider<ContentItemIndex>();

            return services;
        }

        public static void AddWhereInputIndexPropertyProvider<IIndexType>(this IServiceCollection services)
            where IIndexType : MapIndex
        {
            services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<IIndexType>>();
        }

        /// <summary>
        /// Registers a type providing custom filters for content item filters
        /// </summary>
        /// <typeparam name="TObjectTypeToFilter"></typeparam>
        /// <typeparam name="TFilterType"></typeparam>
        /// <param name="services"></param>
        public static void AddGraphQLFilterType<TObjectTypeToFilter, TFilterType>(this IServiceCollection services)
            where TObjectTypeToFilter : class
            where TFilterType : GraphQLFilter<TObjectTypeToFilter>
        {
            services.AddTransient<IGraphQLFilter<TObjectTypeToFilter>, TFilterType>();
        }
    }
}
