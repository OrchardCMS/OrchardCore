using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Modules;

namespace OrchardCore.Layers.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISchemaBuilder, SiteLayersQuery>();
            services.AddTransient<LayerQueryObjectType>();
            services.AddTransient<LayerWidgetQueryObjectType>();
        }
    }
}
