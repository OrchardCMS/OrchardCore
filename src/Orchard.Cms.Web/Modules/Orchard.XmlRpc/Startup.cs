using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Core.XmlRpc.Services;
using Orchard.Environment.Extensions.Features.Attributes;

namespace Orchard.XmlRpc
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
                areaName: "Orchard.XmlRpc",
                template: "xmlrpc",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }

    [OrchardFeature("Orchard.RemotePublishing")]
    public class MetaWeblogStartup  : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "MetaWeblog",
                areaName: "Orchard.XmlRpc",
                template: "xmlrpc/metaweblog",
                defaults: new { controller = "MetaWeblog", action = "Manifest" }
            );
        }
    }
}
