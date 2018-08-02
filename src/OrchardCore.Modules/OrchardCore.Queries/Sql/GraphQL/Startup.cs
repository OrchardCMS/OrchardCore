using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Modules;
using OrchardCore.Queries.GraphQL.Mutations;
using OrchardCore.Queries.GraphQL.Mutations.Types;
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
            services.AddTransient<IQueryFieldTypeProvider, SqlQueryFieldTypeProvider>();

            services.AddGraphMutationType<CreateQueryMutation<SqlQuery>>();
            services.AddTransient<CreateQueryOutcomeType<SqlQuery>>();
        }
    }
}
