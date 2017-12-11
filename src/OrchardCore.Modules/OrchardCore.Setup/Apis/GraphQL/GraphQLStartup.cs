using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Modules;

namespace OrchardCore.Setup.Apis.GraphQL
{
    public class GraphQLStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphMutationType<CreateTenantMutation>();
            services.AddScoped<CreateTenantOutcomeType>();
        }
    }
}
