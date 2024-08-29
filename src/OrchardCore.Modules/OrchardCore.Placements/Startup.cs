using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Placements.Deployment;
using OrchardCore.Placements.Recipes;
using OrchardCore.Placements.Services;
using OrchardCore.Placements.Settings;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Placements;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();

        services.TryAddScoped<IPlacementStore, DatabasePlacementsStore>();
        services.AddScoped<PlacementsManager>();
        services.AddScoped<IShapePlacementProvider, PlacementProvider>();

        // Shortcuts in settings
        services.AddScoped<IContentPartDefinitionDisplayDriver, PlacementContentPartDefinitionDriver>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, PlacementContentTypePartDefinitionDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, PlacementContentPartFieldDefinitionDisplayDriver>();

        // Recipes
        services.AddRecipeExecutionStep<PlacementStep>();
    }
}

[Feature("OrchardCore.Placements.FileStorage")]
public class FileContentDefinitionStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.RemoveAll<IPlacementStore>();
        services.AddScoped<IPlacementStore, FilePlacementsStore>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<PlacementsDeploymentSource, PlacementsDeploymentStep, PlacementsDeploymentStepDriver>();
    }
}
