using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.AdminMenu.Deployment;
using OrchardCore.AdminMenu.Recipes;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddTransient<IDataMigration, Migrations>();

            services.AddScoped<IAdminMenuService, AdminMenuService>();
            services.AddScoped<AdminMenuNavigationProvidersCoordinator, AdminMenuNavigationProvidersCoordinator>();

            services.AddScoped<IDisplayManager<MenuItem>, DisplayManager<MenuItem>>();

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
        }
    }
}
