using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    public class ModularTenantRouteBuilder : IModularTenantRouteBuilder
    {
        // Register one top level TenantRoute per tenant. Each instance contains all the routes
        // for this tenant.
        public ModularTenantRouteBuilder()
        {
        }

        public IRouteBuilder Build(IApplicationBuilder appBuilder)
        {
            var routeBuilder = new RouteBuilder(appBuilder)
            {
                DefaultHandler = appBuilder.ApplicationServices.GetRequiredService<MvcRouteHandler>()
            };

            return routeBuilder;
        }
    }
}