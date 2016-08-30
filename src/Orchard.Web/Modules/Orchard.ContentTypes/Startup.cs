using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Security.Permissions;

namespace Orchard.ContentTypes
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IPermissionProvider, Permissions>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "EditField",
                area: "Orchard.ContentTypes",
                template: "Admin/EditField/{id}/{name}",
                controller: "Admin",
                action: "EditField"
            );

            routes.MapAreaRoute(
                name: "EditTypePart",
                area: "Orchard.ContentTypes",
                template: "Admin/Edit/{id}/{name}",
                controller: "Admin",
                action: "EditTypePart"
            );

            routes.MapAreaRoute(
                name: "RemovePart",
                area: "Orchard.ContentTypes",
                template: "Admin/RemovePart/{id}/{name}",
                controller: "Admin",
                action: "RemovePart"
            );
        }
    }
}
