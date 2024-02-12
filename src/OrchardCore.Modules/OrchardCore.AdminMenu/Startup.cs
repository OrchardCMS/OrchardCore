using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.AdminMenu.Deployment;
using OrchardCore.AdminMenu.Recipes;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Deployment;
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
            services.AddScoped<IAdminMenuPermissionService, AdminMenuPermissionService>();

            services.AddScoped<IAdminMenuService, AdminMenuService>();
            services.AddScoped<AdminMenuNavigationProvidersCoordinator>();

            services.AddRecipeExecutionStep<AdminMenuStep>();

            services.AddDeployment<AdminMenuDeploymentSource, AdminMenuDeploymentStep, AdminMenuDeploymentStepDriver>();

            // placeholder treeNode
            services.AddAdminMenu<PlaceholderAdminNode, PlaceholderAdminNodeNavigationBuilder, PlaceholderAdminNodeDriver>();

            // link treeNode
            services.AddAdminMenu<LinkAdminNode, LinkAdminNodeNavigationBuilder, LinkAdminNodeDriver>();
        }
    }
}
