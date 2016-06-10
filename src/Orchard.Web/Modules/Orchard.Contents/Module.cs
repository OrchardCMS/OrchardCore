using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;

namespace Orchard.Contents
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddContentManagement();
            serviceCollection.AddContentManagementDisplay();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "DisplayContent",
                area: "Orchard.Contents",
                template: "Contents/Item/Display/{id}",
                controller: "Item",
                action: "Display"
            );

            routes.MapAreaRoute(
                name: "PreviewContent",
                area: "Orchard.Contents",
                template: "Contents/Item/Preview/{id}",
                controller: "Item",
                action: "Preview"
            );
        }
    }
}