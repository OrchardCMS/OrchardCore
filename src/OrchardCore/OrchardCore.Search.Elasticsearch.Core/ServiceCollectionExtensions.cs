using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Queries;
using OrchardCore.Recipes;
using OrchardCore.Search.Elasticsearch.Core.Handlers;
using OrchardCore.Search.Elasticsearch.Core.Recipes;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Elastic services.
    /// </summary>
    public static IServiceCollection AddElasticServices(this IServiceCollection services)
    {
        services.AddSingleton<ElasticsearchIndexSettingsService>();
        services.AddSingleton<ElasticsearchIndexManager>();
        services.AddScoped<ElasticsearchIndexingService>();
        services.AddScoped<IModularTenantEvents, ElasticsearchIndexInitializerService>();
        services.AddScoped<IElasticsearchQueryService, ElasticsearchQueryService>();
        services.AddScoped<IContentHandler, ElasticsearchIndexingContentHandler>();

        services.AddQuerySource<ElasticsearchQuerySource>(ElasticsearchQuerySource.SourceName);

        services.AddRecipeExecutionStep<ElasticsearchIndexStep>();
        services.AddRecipeExecutionStep<ElasticsearchSettingsStep>();
        services.AddRecipeExecutionStep<ElasticsearchIndexRebuildStep>();
        services.AddRecipeExecutionStep<ElasticsearchIndexResetStep>();

        return services;
    }
}
