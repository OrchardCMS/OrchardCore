using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.Modules;
using OrchardCore.RemoteManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Drivers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Recipes.RecipeSteps;
using OrchardCore.Recipes.Services;
using OrchardCore.Recipes.Endpoints.Management;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Recipes;

/// <summary>
/// These services are registered on the tenant service collection.
/// </summary>
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRemoteManagementCapabilityProvider, RecipeRemoteManagementCapabilityProvider>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<RecipesPermissionProvider>();

        // Builds the rows of the recipes admin list.
        services.AddDisplayDriver<RecipeEntry, RecipeEntryDisplayDriver>();
        services.AddRecipeExecutionStep<CommandStep>();
        services.AddRecipeExecutionStep<RecipesStep>();
        services.AddRecipeExecutionStep<ReloadTenantStep>();

        services.AddDeploymentTargetHandler<RecipeDeploymentTargetHandler>();

    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddRecipeManagementEndpoints();
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
