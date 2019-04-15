using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.HomeRoute.Routing;
using OrchardCore.Modules;
using OrchardCore.Routing;

namespace OrchardCore.HomeRoute
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HomeRoute>();
            services.AddSingleton<IEndpointAddressScheme<RouteValuesAddress>, HomeRouteValuesAddressScheme>();
            services.AddSingleton<IShellRoutingFilter, HomeRouteRoutingFilter>();
        }
    }
}
