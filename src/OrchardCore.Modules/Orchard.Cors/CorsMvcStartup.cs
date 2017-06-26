using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Cors;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Orchard.Cors
{
    [Feature("Orchard.Cors.Mvc")]
    public class CorsMvcStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, CorsApplicationModelProvider>());
            services.TryAddTransient<CorsAuthorizationFilter, CorsAuthorizationFilter>();
        }
    }
}