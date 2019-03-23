using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    public class ConfigureTenantPipeline : IConfigureTenantPipeline
    {
        public ConfigureTenantPipeline()
        {
        }

        public void Configure(IApplicationBuilder appBuilder, Action<IRouteBuilder> configureRoutes)
        {
            appBuilder.UseMvc(configureRoutes);
        }
    }
}