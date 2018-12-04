using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminTrees.Services;
using OrchardCore.AdminTrees.AdminNodes;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using YesSql.Indexes;
using OrchardCore.Deployment;
using OrchardCore.AdminTrees.Deployment;
using OrchardCore.Recipes;
using OrchardCore.AdminTrees.Recipes;

namespace OrchardCore.AdminTrees
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddTransient<IDataMigration, Migrations>();

            services.AddSingleton<IAdminTreeService, AdminTreeService>();
            services.AddScoped<AdminTreeNavigationProvidersCoordinator, AdminTreeNavigationProvidersCoordinator>();

            services.AddScoped<IDisplayManager<MenuItem>, DisplayManager<MenuItem>>();

            services.AddRecipeExecutionStep<AdminTreeStep>();

            services.AddTransient<IDeploymentSource, AdminTreesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AdminTreesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AdminTreesDeploymentStepDriver>();


            // placeholder treeNode
            services.AddSingleton<IAdminNodeProviderFactory>(new AdminNodeProviderFactory<PlaceholderAdminNode>());
            services.AddScoped<IAdminNodeNavigationBuilder, PlaceholderAdminNodeNavigationBuilder>();
            services.AddScoped<IDisplayDriver<MenuItem>, PlaceholderAdminNodeDriver>();

            // link treeNode
            services.AddSingleton<IAdminNodeProviderFactory>(new AdminNodeProviderFactory<LinkAdminNode>());
            services.AddScoped<IAdminNodeNavigationBuilder, LinkAdminNodeNavigationBuilder>();
            services.AddScoped<IDisplayDriver<MenuItem>, LinkAdminNodeDriver>();

        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}