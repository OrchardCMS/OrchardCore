using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Modules
{
    public class ConfigureTenantPipeline : IConfigureTenantPipeline
    {
        public ConfigureTenantPipeline()
        {
        }

        public void Configure(IApplicationBuilder appBuilder, Action<IRouteBuilder> configureRoutes)
        {
            var routeBuilder = new RouteBuilder(appBuilder);

            configureRoutes(routeBuilder);

            appBuilder.UseRouter(routeBuilder.Build());
        }
    }
}