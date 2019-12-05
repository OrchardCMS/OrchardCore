using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment.Remote;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Navigation;
using OrchardCore.Admin;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment.Remote.Controllers;

namespace OrchardCore.Deployment
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<RemoteInstanceService>();
            services.AddScoped<RemoteClientService>();
            services.AddScoped<IDeploymentTargetProvider, RemoteInstanceDeploymentTargetProvider>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // RemoteClient
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteClient",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Index",
                defaults: new { controller = typeof(RemoteClientController).ControllerName(), action = nameof(RemoteClientController.Index) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteClientCreate",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Create",
                defaults: new { controller = typeof(RemoteClientController).ControllerName(), action = nameof(RemoteClientController.Create) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteClientDelete",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Delete/{id}",
                defaults: new { controller = typeof(RemoteClientController).ControllerName(), action = nameof(RemoteClientController.Delete) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteClientEdit",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Edit/{id}",
                defaults: new { controller = typeof(RemoteClientController).ControllerName(), action = nameof(RemoteClientController.Edit) }
            );

            // Remote Instances
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteInstances",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteInstance/Index",
                defaults: new { controller = typeof(RemoteInstanceController).ControllerName(), action = nameof(RemoteInstanceController.Index) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteInstancesCreate",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Create",
                defaults: new { controller = typeof(RemoteInstanceController).ControllerName(), action = nameof(RemoteInstanceController.Create) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteInstancesDelete",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Delete/{id}",
                defaults: new { controller = typeof(RemoteInstanceController).ControllerName(), action = nameof(RemoteClientController.Delete) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteInstancesEdit",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Edit/{id}",
                defaults: new { controller = typeof(RemoteInstanceController).ControllerName(), action = nameof(RemoteInstanceController.Edit) }
            );
        }
    }
}
