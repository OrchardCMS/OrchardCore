using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Providers;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Lucene queries services.
        /// </summary>
        public static IServiceCollection AddElasticQueries(this IServiceCollection services)
        {
            services.AddScoped<SearchProvider, ElasticSearchProvider>();
            services.AddScoped<IElasticQueryService, ElasticQueryService>();
            return services;
        }
    }
}
