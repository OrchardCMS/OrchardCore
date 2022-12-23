using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.AdminMenu.Controllers;
using OrchardCore.AdminMenu.Deployment;
using OrchardCore.AdminMenu.Recipes;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu
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
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IAdminMenuPermissionService, AdminMenuPermissionService>();

            services.AddScoped<IAdminMenuService, AdminMenuService>();
            services.AddScoped<AdminMenuNavigationProvidersCoordinator>();

            services.AddRecipeExecutionStep<AdminMenuStep>();

            services.AddTransient<IDeploymentSource, AdminMenuDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AdminMenuDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AdminMenuDeploymentStepDriver>();

            // placeholder treeNode
            services.AddSingleton<IAdminNodeProviderFactory>(new AdminNodeProviderFactory<PlaceholderAdminNode>());
            services.AddScoped<IAdminNodeNavigationBuilder, PlaceholderAdminNodeNavigationBuilder>();
            services.AddScoped<IDisplayDriver<MenuItem>, PlaceholderAdminNodeDriver>();

            // link treeNode
            services.AddSingleton<IAdminNodeProviderFactory>(new AdminNodeProviderFactory<LinkAdminNode>());
            services.AddScoped<IAdminNodeNavigationBuilder, LinkAdminNodeNavigationBuilder>();
            services.AddScoped<IDisplayDriver<MenuItem>, LinkAdminNodeDriver>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Menu
            var menuControllerName = typeof(MenuController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "AdminMenuList",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/List",
                defaults: new { controller = menuControllerName, action = nameof(MenuController.List) }
            );
            routes.MapAreaControllerRoute(
                name: "AdminMenuCreate",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Create",
                defaults: new { controller = menuControllerName, action = nameof(MenuController.Create) }
            );
            routes.MapAreaControllerRoute(
                name: "AdminMenuDelete",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Delete/{id}",
                defaults: new { controller = menuControllerName, action = nameof(MenuController.Delete) }
            );
            routes.MapAreaControllerRoute(
                name: "AdminMenuEdit",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Edit/{id}",
                defaults: new { controller = menuControllerName, action = nameof(MenuController.Edit) }
            );
            routes.MapAreaControllerRoute(
                name: "AdminMenuToggle",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Toggle/{id}",
                defaults: new { controller = menuControllerName, action = nameof(MenuController.Toggle) }
            );

            // Node
            var nodeControllerName = typeof(NodeController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "AdminMenuNodeList",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Node/List",
                defaults: new { controller = nodeControllerName, action = nameof(NodeController.List) }
            );
            routes.MapAreaControllerRoute(
                name: "AdminMenuNodeCreate",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Node/Create",
                defaults: new { controller = nodeControllerName, action = nameof(NodeController.Create) }
            );
            routes.MapAreaControllerRoute(
                name: "AdminMenuNodeDelete",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Node/Delete",
                defaults: new { controller = nodeControllerName, action = nameof(NodeController.Delete) }
            );
            routes.MapAreaControllerRoute(
                name: "AdminMenuNodeEdit",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Node/Edit",
                defaults: new { controller = nodeControllerName, action = nameof(NodeController.Edit) }
            );
            routes.MapAreaControllerRoute(
                name: "AdminMenuNodeToggle",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Node/Toggle",
                defaults: new { controller = nodeControllerName, action = nameof(NodeController.Toggle) }
            );
            routes.MapAreaControllerRoute(
                name: "AdminMenuNodeMoveNode",
                areaName: "OrchardCore.AdminMenu",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminMenu/Node/MoveNode",
                defaults: new { controller = nodeControllerName, action = nameof(NodeController.MoveNode) }
            );
        }
    }
}
