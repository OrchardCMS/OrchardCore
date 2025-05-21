using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Modules;
using OrchardCore.Queries;
using OrchardCore.Recipes;
using OrchardCore.Search.Elasticsearch.Core.Recipes;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Elastic services.
    /// </summary>
    public static IServiceCollection AddElasticsearchServices(this IServiceCollection services)
    {
        services.AddScoped<ElasticsearchIndexSettingsService>();
        services.AddSingleton<ElasticsearchIndexManager>();
        services.AddScoped<ElasticsearchQueryService>();
        services.AddScoped<ElasticsearchQueryService>();
        services.AddSingleton<ElasticsearchIndexNameService>();

        services.AddScoped<IModularTenantEvents, ElasticsearchIndexInitializerService>();

        services.AddQuerySource<ElasticsearchQuerySource>(ElasticsearchQuerySource.SourceName);

        services.AddRecipeExecutionStep<ElasticsearchIndexStep>();
        services.AddRecipeExecutionStep<ElasticsearchSettingsStep>();
        services.AddRecipeExecutionStep<ElasticsearchIndexRebuildStep>();
        services.AddRecipeExecutionStep<ElasticsearchIndexResetStep>();

        services.AddTransient<IConfigureOptions<DocumentJsonSerializerOptions>, DocumentJsonSerializerOptionsConfiguration>();

        return services;
    }
}
