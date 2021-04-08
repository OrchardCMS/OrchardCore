using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Search.Elastic.QueryProviders;

namespace OrchardCore.Search.Elastic
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Lucene queries services.
        /// </summary>
        public static IServiceCollection AddLuceneQueries(this IServiceCollection services)
        {
            services.AddScoped<IElasticQueryService, ElasticQueryService>();

            services.AddSingleton<IElasticQueryProvider, BooleanQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, FuzzyQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, MatchQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, MatchAllQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, MatchPhraseQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, QueryStringQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, PrefixQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, RangeQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, RegexpQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, SimpleQueryStringQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, TermQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, TermsQueryProvider>();
            services.AddSingleton<IElasticQueryProvider, WildcardQueryProvider>();
            return services;
        }
    }
}
