using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Roles.Controllers;
using OrchardCore.Roles.Deployment;
using OrchardCore.Roles.Recipes;
using OrchardCore.Roles.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles
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
            services.TryAddScoped<RoleManager<IRole>>();
            services.TryAddScoped<IRoleStore<IRole>, RoleStore>();
            services.TryAddScoped<IRoleService, RoleService>();
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

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "RolesIndex",
                areaName: "OrchardCore.Roles",
                pattern: _adminOptions.AdminUrlPrefix + "/Roles/Index",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );
            routes.MapAreaControllerRoute(
                name: "RolesCreate",
                areaName: "OrchardCore.Roles",
                pattern: _adminOptions.AdminUrlPrefix + "/Roles/Create",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Create) }
            );
            routes.MapAreaControllerRoute(
                name: "RolesDelete",
                areaName: "OrchardCore.Roles",
                pattern: _adminOptions.AdminUrlPrefix + "/Roles/Delete/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
            );
            routes.MapAreaControllerRoute(
                name: "RolesEdit",
                areaName: "OrchardCore.Roles",
                pattern: _adminOptions.AdminUrlPrefix + "/Roles/Edit/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Edit) }
            );
        }
    }
}
