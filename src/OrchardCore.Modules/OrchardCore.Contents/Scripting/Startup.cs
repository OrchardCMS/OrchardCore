using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Scripting;

namespace OrchardCore.Contents.Scripting
{
    [RequireFeatures("OrchardCore.Scripting")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGlobalMethodProvider, ContentMethodsProvider>();
            services.AddSingleton<IGlobalMethodProvider, UrlMethodsProvider>();
        }
    }
}
