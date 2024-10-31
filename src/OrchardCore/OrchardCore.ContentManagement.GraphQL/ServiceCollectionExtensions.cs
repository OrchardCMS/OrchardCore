using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentTypes.Events;
using OrchardCore.Security.Permissions;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement.GraphQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContentGraphQL(this IServiceCollection services)
    {
        services.AddSingleton<ISchemaBuilder, ContentItemQuery>();
        services.AddSingleton<ISchemaBuilder, ContentTypeQuery>();
        services.AddTransient<ContentItemInterface>();

        services.AddTransient<ContentItemType>();

        services.AddPermissionProvider<Permissions>();

        services.AddTransient<DynamicPartGraphType>();
        services.AddScoped<IContentTypeBuilder, TypedContentTypeBuilder>();
        services.AddScoped<IContentTypeBuilder, DynamicContentTypeQueryBuilder>();

        services.AddOptions<GraphQLContentOptions>();
        services.AddGraphQLFilterType<ContentItem, ContentItemFilters>();
        services.AddWhereInputIndexPropertyProvider<ContentItemIndex>();

        return services;
    }

    public static IServiceCollection AddContentFieldsInputGraphQL(this IServiceCollection services)
    {
        services.AddScoped<DynamicContentFieldsIndexAliasProvider>()
            .AddScoped<IIndexAliasProvider>(sp => sp.GetService<DynamicContentFieldsIndexAliasProvider>())
            .AddScoped<IContentDefinitionEventHandler>(sp => sp.GetService<DynamicContentFieldsIndexAliasProvider>());

        services.AddScoped<IContentTypeBuilder, DynamicContentTypeWhereInputBuilder>();

        return services;
    }

    public static void AddWhereInputIndexPropertyProvider<TIndexType>(this IServiceCollection services)
        where TIndexType : MapIndex
    {
        services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<TIndexType>>();
    }

    /// <summary>
    /// Registers a type providing custom filters for content item filters.
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
