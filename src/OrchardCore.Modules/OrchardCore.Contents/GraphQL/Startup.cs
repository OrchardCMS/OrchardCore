using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.GraphQL;
using OrchardCore.Modules;

namespace OrchardCore.Contents.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentGraphQL();
        }
    }
}
