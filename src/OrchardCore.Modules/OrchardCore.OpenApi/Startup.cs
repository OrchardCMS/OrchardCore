using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.OpenApi;

namespace OrchardCore.Swagger
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IOpenApiDefinition, OrchardApiDefinition>();
        }
    }
}