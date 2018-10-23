using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Modules;
using OrchardCore.Title.Model;

namespace OrchardCore.Title.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQLInputType<TitlePart, TitleInputObjectType>();
            services.AddGraphQLQueryType<TitlePart, TitleQueryObjectType>();
        }
    }
}
