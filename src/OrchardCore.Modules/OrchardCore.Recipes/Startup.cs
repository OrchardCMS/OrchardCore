using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.Modules;
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
        services.AddRecipeExecutionStep<CommandStep>();
        services.AddRecipeExecutionStep<RecipesStep>();

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
