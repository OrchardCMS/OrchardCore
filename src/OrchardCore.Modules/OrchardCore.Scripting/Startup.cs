using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting.JavaScript;

namespace OrchardCore.Scripting
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScripting();
            services.AddJavaScriptEngine();
        }
    }
}
