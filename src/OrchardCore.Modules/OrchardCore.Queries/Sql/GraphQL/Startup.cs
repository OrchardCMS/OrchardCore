using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Modules;
using OrchardCore.Queries.Sql.GraphQL.Queries;

namespace OrchardCore.Queries.Sql.GraphQL
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    [Feature("OrchardCore.Queries.Sql")]
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISchemaBuilder, SqlQueryFieldTypeProvider>();
        }
    }
}
