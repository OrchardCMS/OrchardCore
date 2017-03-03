using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Scripting.JavaScript;

namespace Orchard.Scripting
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
