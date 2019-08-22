using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment.Remote;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Navigation;

namespace OrchardCore.Deployment
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<RemoteInstanceService>();
            services.AddScoped<RemoteClientService>();
            services.AddScoped<IDeploymentTargetProvider, RemoteInstanceDeploymentTargetProvider>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Deployment.Remote.RemoteInstances",
                areaName: "OrchardCore.Deployment.Remote",
                template: "Admin/OrchardCore.Deployment.Remote/RemoteInstance/Index",
                defaults: new { controller = "RemoteInstance", action = "Index" }
            );

            routes.MapAreaRoute(
                name: "Deployment.Remote.RemoteClients",
                areaName: "OrchardCore.Deployment.Remote",
                template: "Admin/OrchardCore.Deployment.Remote/RemoteClient/Index",
                defaults: new { controller = "RemoteClient", action = "Index" }
            );
        }
    }
}
