using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Modules;
using OrchardCore.Queries.GraphQL.Mutations;
using OrchardCore.Queries.GraphQL.Mutations.Types;
using OrchardCore.Queries.Lucene.GraphQL.Queries;

namespace OrchardCore.Lucene.GraphQL
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISchemaBuilder, LuceneQueryFieldTypeProvider>();

            services.AddGraphMutationType<CreateQueryMutation<LuceneQuery>>();
            services.AddTransient<CreateQueryOutcomeType<LuceneQuery>>();
        }
    }
}
