using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    public class ModularTenantRouteBuilder : IModularTenantRouteBuilder
    {
        public ModularTenantRouteBuilder()
        {
        }

        public void Build(IApplicationBuilder appBuilder, Action<IRouteBuilder> configureRoutes)
        {
            appBuilder.UseMvc(configureRoutes);
        }
    }
}