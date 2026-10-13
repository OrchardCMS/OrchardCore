using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps;

/// <summary>
/// Recipe step that requests a tenant reload on completion of the recipe. The settings written by a recipe
/// usually apply without a reload, but this step lets a recipe activate custom settings that are only read when
/// the tenant is built. Place it after the steps whose settings need to become live.
/// </summary>
public sealed class ReloadTenantStep : NamedRecipeStepHandler
{
    private readonly IShellReleaseManager _shellReleaseManager;

    public ReloadTenantStep(IShellReleaseManager shellReleaseManager)
        : base("ReloadTenant")
    {
        _shellReleaseManager = shellReleaseManager;
    }

    protected override Task HandleAsync(RecipeExecutionContext context)
    {
        _shellReleaseManager.RequestRelease();

        return Task.CompletedTask;
    }
}
