using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Drivers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Recipes.RecipeSteps;
using OrchardCore.Recipes.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Recipes;

/// <summary>
/// These services are registered on the tenant service collection.
/// </summary>
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<RecipesPermissionProvider>();

        // Builds the rows of the recipes admin list.
        services.AddDisplayDriver<RecipeEntry, RecipeEntryDisplayDriver>();
        services.AddRecipeExecutionStep<CommandStep>();
        services.AddRecipeExecutionStep<RecipesStep>();
        services.AddRecipeExecutionStep<ReloadTenantStep>();

        services.AddDeploymentTargetHandler<RecipeDeploymentTargetHandler>();
    }
}

[Feature("OrchardCore.Recipes.Core")]
public sealed class RecipesCoreStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipes();
    }
}
