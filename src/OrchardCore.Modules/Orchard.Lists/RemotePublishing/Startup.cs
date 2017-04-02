using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Orchard.Environment.Extensions.Features.Attributes;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.Core.XmlRpc;

namespace Orchard.Lists.RemotePublishing
{

    [OrchardFeature("Orchard.RemotePublishing")]
    public class RemotePublishingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IXmlRpcHandler, MetaWeblogHandler>();
            services.AddScoped<IContentPartDisplayDriver, ListMetaWeblogDriver>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "RSD",
                areaName: "Orchard.Lists",
                template: "xmlrpc/metaweblog/{contentItemId}/rsd",
                defaults: new { controller = "RemotePublishing", action = "Rsd" }
            );
        }
    }
}
