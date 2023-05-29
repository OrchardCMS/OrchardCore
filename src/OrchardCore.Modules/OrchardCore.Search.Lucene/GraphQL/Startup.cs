using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Modules;
using OrchardCore.Queries.Lucene.GraphQL.Queries;

namespace OrchardCore.Search.Lucene.GraphQL
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
        }
    }
}
