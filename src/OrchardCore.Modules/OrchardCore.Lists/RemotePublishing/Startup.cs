using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.XmlRpc;
using OrchardCore.Lists.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Lists.RemotePublishing
{

    [RequireFeatures("OrchardCore.RemotePublishing")]
    public class RemotePublishingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IXmlRpcHandler, MetaWeblogHandler>();
            services.AddContentPart<ListPart>()
                .UseDisplayDriver<ListMetaWeblogDriver>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "RSD",
                areaName: "OrchardCore.Lists",
                pattern: "xmlrpc/metaweblog/{contentItemId}/rsd",
                defaults: new { controller = "RemotePublishing", action = "Rsd" }
            );
        }
    }
}
