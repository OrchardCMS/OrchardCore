using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Modules;
using OrchardCore.Queries.GraphQL.Mutations;

namespace OrchardCore.Queries.GraphQL
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphMutationType<DeleteQueryMutation>();
            services.AddTransient<DeletionStatusObjectGraphType>();
        }
    }
}
