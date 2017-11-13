using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Lucene;
using OrchardCore.Modules;
using OrchardCore.Queries.Apis.GraphQL.Mutations;
using OrchardCore.Queries.Apis.GraphQL.Mutations.Types;
using OrchardCore.Queries.Apis.GraphQL.Queries;
using OrchardCore.Queries.Lucene.Apis.GraphQL.Queries;

namespace OrchardCore.Queries.Lucene.Apis.GraphQL
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    [RequireFeatures("OrchardCore.Lucene")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDynamicQueryFieldTypeProvider, LuceneQueryFieldTypeProvider>();

            services.AddGraphMutationType<CreateQueryMutation<LuceneQuery>>();
            services.AddScoped<CreateQueryOutcomeType<LuceneQuery>>();

            //services.AddGraphQueryType<QueriesQuery<LuceneQueryQuery>>();
            //services.AddScoped<LuceneQueryQuery>();
        }
    }
}
