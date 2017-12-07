using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Apis.JsonApi;
using OrchardCore.Modules;

namespace OrchardCore.Contents.JsonApi
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IJsonApiResultManager, ContentJsonApiResultManager>();
        }
    }
}
