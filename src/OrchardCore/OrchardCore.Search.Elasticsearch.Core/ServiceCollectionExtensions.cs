using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Queries;
using OrchardCore.Recipes;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Handlers;
using OrchardCore.Search.Elasticsearch.Core.Providers;
using OrchardCore.Search.Elasticsearch.Core.Recipes;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Elastic services.
        /// </summary>
        public static IServiceCollection AddElasticServices(this IServiceCollection services)
        {
            services.AddSingleton<SearchProvider, ElasticSearchProvider>();
            services.AddSingleton<ElasticIndexSettingsService>();
            services.AddSingleton<ElasticIndexManager>();
            services.AddSingleton<ElasticAnalyzerManager>();
            services.AddScoped<ElasticIndexingService>();
            services.AddScoped<IElasticSearchQueryService, ElasticSearchQueryService>();
            services.AddScoped<IElasticQueryService, ElasticQueryService>();
            services.AddScoped<IContentHandler, ElasticIndexingContentHandler>();

            // LuceneQuerySource is registered for both the Queries module and local usage
            services.AddScoped<IQuerySource, ElasticQuerySource>();
            services.AddScoped<ElasticQuerySource>();
            services.AddRecipeExecutionStep<ElasticIndexStep>();

            return services;
        }
    }
}
