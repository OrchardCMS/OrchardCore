using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Deployment.Remote;
using Orchard.Deployment.Remote.Services;
using Orchard.Environment.Navigation;

namespace Orchard.Deployment
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
        }
    }
}
