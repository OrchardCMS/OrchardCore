using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "XmlRpc",
                areaName: "OrchardCore.XmlRpc",
                template: "xmlrpc",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }

    [Feature("OrchardCore.RemotePublishing")]
    public class MetaWeblogStartup  : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "MetaWeblog",
                areaName: "OrchardCore.XmlRpc",
                template: "xmlrpc/metaweblog",
                defaults: new { controller = "MetaWeblog", action = "Manifest" }
            );
        }
    }
}
