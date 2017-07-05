using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Cors
{
    public class CorsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
        }
    }
}
