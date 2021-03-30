using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Lucene.QueryProviders;
using OrchardCore.Lucene.QueryProviders.Filters;

namespace OrchardCore.Lucene
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Lucene queries services.
        /// </summary>
        public static IServiceCollection AddLuceneQueries(this IServiceCollection services)
        {
            services.AddScoped<ILuceneQueryService, LuceneQueryService>();

            services.AddSingleton<ILuceneQueryProvider, BooleanQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, FuzzyQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, MatchQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, MatchAllQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, MatchPhraseQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, QueryStringQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, PrefixQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, RangeQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, RegexpQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, SimpleQueryStringQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, TermQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, TermsQueryProvider>();
            services.AddSingleton<ILuceneQueryProvider, WildcardQueryProvider>();

            services.AddSingleton<ILuceneBooleanFilterProvider, FuzzyFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, GeoBoundingBoxFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, GeoDistanceFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, MatchFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, MatchPhraseFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, MatchAllFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, PrefixFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, RangeFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, TermFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, TermsFilterProvider>();
            services.AddSingleton<ILuceneBooleanFilterProvider, WildcardFilterProvider>();
            
            return services;
        }
    }
}
