using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Navigation;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes;
using OrchardCore.Roles.Recipes;
using OrchardCore.Roles.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using OrchardCore.Deployment;
using OrchardCore.Roles.Deployment;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Roles
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddScoped<RoleManager<IRole>>();
            services.TryAddScoped<IRoleStore<IRole>, RoleStore>();
            services.TryAddScoped<IRoleProvider, RoleStore>();
            services.TryAddScoped<IRoleClaimStore<IRole>, RoleStore>();
            services.AddRecipeExecutionStep<RolesStep>();

            services.AddScoped<IFeatureEventHandler, RoleUpdater>();
            services.AddScoped<IAuthorizationHandler, RolesPermissionsHandler>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            // Deployment
            services.AddTransient<IDeploymentSource, AllRolesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllRolesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllRolesDeploymentStepDriver>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
