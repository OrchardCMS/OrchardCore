using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Queries;
using OrchardCore.Recipes;
using OrchardCore.Search.Elasticsearch.Core.Handlers;
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
            services.AddSingleton<ElasticIndexSettingsService>();
            services.AddSingleton<ElasticIndexManager>();
            services.AddScoped<ElasticIndexingService>();
            services.AddScoped<IModularTenantEvents, ElasticIndexInitializerService>();
            services.AddScoped<IElasticSearchQueryService, ElasticSearchQueryService>();
            services.AddScoped<IElasticQueryService, ElasticQueryService>();
            services.AddScoped<IContentHandler, ElasticIndexingContentHandler>();

            // ElasticQuerySource is registered for both the Queries module and local usage
            services.AddScoped<IQuerySource, ElasticQuerySource>();
            services.AddScoped<ElasticQuerySource>();
            services.AddRecipeExecutionStep<ElasticIndexStep>();
            services.AddRecipeExecutionStep<ElasticSettingsStep>();
            services.AddRecipeExecutionStep<ElasticIndexRebuildStep>();
            services.AddRecipeExecutionStep<ElasticIndexResetStep>();

            return services;
        }
    }
}
