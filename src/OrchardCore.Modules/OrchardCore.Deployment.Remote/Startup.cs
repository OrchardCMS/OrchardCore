using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment.Remote;
using OrchardCore.Deployment.Remote.Controllers;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

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
            services.AddScoped<IPermissionProvider, Permissions>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // RemoteClient
            var remoteClientControllerName = typeof(RemoteClientController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteClient",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Index",
                defaults: new { controller = remoteClientControllerName, action = nameof(RemoteClientController.Index) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteClientCreate",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Create",
                defaults: new { controller = remoteClientControllerName, action = nameof(RemoteClientController.Create) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteClientDelete",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Delete/{id}",
                defaults: new { controller = remoteClientControllerName, action = nameof(RemoteClientController.Delete) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteClientEdit",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteClient/Edit/{id}",
                defaults: new { controller = remoteClientControllerName, action = nameof(RemoteClientController.Edit) }
            );

            // Remote Instances
            var remoteInstanceControllerName = typeof(RemoteInstanceController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteInstances",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteInstance/Index",
                defaults: new { controller = remoteInstanceControllerName, action = nameof(RemoteInstanceController.Index) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteInstancesCreate",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteInstance/Create",
                defaults: new { controller = remoteInstanceControllerName, action = nameof(RemoteInstanceController.Create) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteInstancesDelete",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteInstance/Delete/{id}",
                defaults: new { controller = remoteInstanceControllerName, action = nameof(RemoteClientController.Delete) }
            );
            routes.MapAreaControllerRoute(
                name: "DeploymentRemoteInstancesEdit",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/RemoteInstance/Edit/{id}",
                defaults: new { controller = remoteInstanceControllerName, action = nameof(RemoteInstanceController.Edit) }
            );

            //ExportRemoteInstance
            routes.MapAreaControllerRoute(
                name: "DeploymentExportRemoteInstanceExecute",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: _adminOptions.AdminUrlPrefix + "/Deployment/ExportRemoteInstance/Execute",
                defaults: new { controller = typeof(ExportRemoteInstanceController).ControllerName(), action = nameof(ExportRemoteInstanceController.Execute) }
            );
        }
    }
}
