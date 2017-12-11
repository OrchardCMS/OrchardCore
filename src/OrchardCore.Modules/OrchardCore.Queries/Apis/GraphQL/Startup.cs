using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Modules;
using OrchardCore.Queries.Apis.GraphQL.Mutations;

namespace OrchardCore.Queries.Apis.GraphQL
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphMutationType<DeleteQueryMutation>();
            services.AddScoped<DeletionStatusObjectGraphType>();
        }
    }
}
