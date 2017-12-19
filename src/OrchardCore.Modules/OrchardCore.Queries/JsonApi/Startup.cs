using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.JsonApi;
using OrchardCore.Modules;

namespace OrchardCore.Queries.JsonApi
{
    [RequireFeatures("OrchardCore.Apis.JsonApi")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IJsonApiResultProvider, QueriesJsonApiResultProvider>();
        }
    }
}
