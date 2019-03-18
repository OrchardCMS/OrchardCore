using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Modules
{
    public class ModularTenantRouteBuilder : IModularTenantRouteBuilder
    {
        public ModularTenantRouteBuilder()
        {
        }

        public void Build(IApplicationBuilder appBuilder, Action<IRouteBuilder> configureRoutes)
        {
            var routeBuilder = new RouteBuilder(appBuilder);

            configureRoutes(routeBuilder);

            appBuilder.UseRouter(routeBuilder.Build());
        }
    }
}