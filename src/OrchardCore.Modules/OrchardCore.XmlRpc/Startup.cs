using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.XmlRpc.Services;

namespace OrchardCore.XmlRpc
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IXmlRpcReader, XmlRpcReader>();
            services.AddScoped<IXmlRpcWriter, XmlRpcWriter>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "XmlRpc",
                areaName: "OrchardCore.XmlRpc",
                pattern: "xmlrpc",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }

    [Feature("OrchardCore.RemotePublishing")]
    public class MetaWeblogStartup : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "MetaWeblog",
                areaName: "OrchardCore.XmlRpc",
                pattern: "xmlrpc/metaweblog",
                defaults: new { controller = "MetaWeblog", action = "Manifest" }
            );
        }
    }
}
