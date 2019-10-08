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

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "DeploymentImport",
                areaName: "OrchardCore.Deployment.Remote",
                pattern: "Deployment/Import",
                defaults: new { controller = "ImportRemoteInstance", action = "Import" }
            );
        }
    }
}
