using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Features.Deployment;
using OrchardCore.Features.Recipes.Executors;
using OrchardCore.Features.Services;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Features
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRecipeExecutionStep<FeatureStep>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<INavigationProvider, AdminMenu>();


            services.AddTransient<IDeploymentSource, AllFeaturesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllFeaturesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllFeaturesDeploymentStepDriver>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
