using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Modules
{
    public interface IModularTenantRouteBuilder
    {
        void Build(IApplicationBuilder appBuilder, Action<IRouteBuilder> configureRoutes);
    }
}