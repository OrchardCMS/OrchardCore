using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Scripting.Providers;

namespace OrchardCore.Scripting
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddJavaScriptEngine();
            services.AddSingleton<IGlobalMethodProvider, LogProvider>();
        }
    }
}
