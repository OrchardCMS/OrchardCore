using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminMenu.AdminNodes;
using OrchardCore.AdminMenu.Recipes;
using OrchardCore.AdminMenu.Services;
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

#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRecipeExecutionStep<AdminMenuStep>();
#pragma warning restore CS0618 // Type or member is obsolete
        services.AddRecipeDeploymentStep<AdminMenuRecipeStep>();

        // placeholder treeNode
        services.AddAdminNode<PlaceholderAdminNode, PlaceholderAdminNodeNavigationBuilder, PlaceholderAdminNodeDriver>();

        // link treeNode
        services.AddAdminNode<LinkAdminNode, LinkAdminNodeNavigationBuilder, LinkAdminNodeDriver>();
    }
}

[RequireFeatures("OrchardCore.Deployment", "OrchardCore.AdminMenu")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
    }
}

[RequireFeatures("OrchardCore.DataLocalization")]
public sealed class DataLocalizationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ILocalizationDataProvider, AdminMenuDataLocalizationProvider>();
        services.AddScoped<ILocalizationDataProvider, ListsAdminNodeDataLocalizationProvider>();
        services.AddScoped<ILocalizationDataProvider, PlaceholderAdminNodeDataLocalizationProvider>();
    }
}
