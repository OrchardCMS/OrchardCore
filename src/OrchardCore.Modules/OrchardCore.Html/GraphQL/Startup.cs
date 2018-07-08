using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Body.Model;
using OrchardCore.Modules;

namespace OrchardCore.Body.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQLInputType<BodyPart, BodyInputObjectType>();
            services.AddGraphQLQueryType<BodyPart, BodyQueryObjectType>();
        }
    }
}
