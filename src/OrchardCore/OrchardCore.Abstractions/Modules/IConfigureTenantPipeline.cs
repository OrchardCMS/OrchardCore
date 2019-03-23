using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Modules
{
    public interface IConfigureTenantPipeline
    {
        void Configure(IApplicationBuilder appBuilder, Action<IRouteBuilder> configureRoutes);
    }
}