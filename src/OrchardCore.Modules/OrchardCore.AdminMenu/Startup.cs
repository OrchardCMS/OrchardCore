using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.AdminMenu.Deployment;
using OrchardCore.AdminMenu.Recipes;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.Localization.Data;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddScoped<IAdminMenuService, AdminMenuService>();
        services.AddScoped<IAdminMenuAccessor, AdminMenuAccessor>();
        services.AddScoped<AdminMenuNavigationProvidersCoordinator>();

        services.AddRecipeExecutionStep<AdminMenuStep>();

        services.AddDeployment<AdminMenuDeploymentSource, AdminMenuDeploymentStep, AdminMenuDeploymentStepDriver>();

        // placeholder treeNode
        services.AddAdminNode<PlaceholderAdminNode, PlaceholderAdminNodeNavigationBuilder, PlaceholderAdminNodeDriver>();

        // link treeNode
        services.AddAdminNode<LinkAdminNode, LinkAdminNodeNavigationBuilder, LinkAdminNodeDriver>();

        //migrate admin menu to 3.0 format
        services.AddDataMigration<Migrations>();
    }
}

[RequireFeatures("OrchardCore.DataLocalization")]
public sealed class DataLocalizationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ILocalizationDataProvider, AdminMenuDataLocalizationProvider>();
        services.AddScoped<ILocalizationDataProvider, LinkAdminNodeDataLocalizationProvider>();
        services.AddScoped<ILocalizationDataProvider, PlaceholderAdminNodeDataLocalizationProvider>();
    }
}
