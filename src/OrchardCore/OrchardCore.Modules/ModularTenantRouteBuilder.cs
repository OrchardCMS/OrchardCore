using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Modules
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
            };

            return routeBuilder;
        }

        public void Configure(IRouteBuilder builder)
        {
        }
    }
}