using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Security.Permissions;

namespace Orchard.Admin
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IFilterMetadata, AdminFilter>();
            serviceCollection.AddScoped<IFilterMetadata, AdminMenuFilter>();
            serviceCollection.AddScoped<IPermissionProvider, Permissions>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Admin",
                area: "Orchard.Admin",
                template: "admin",
                controller: "Admin",
                action: "Index"
            );
        }
    }
}
