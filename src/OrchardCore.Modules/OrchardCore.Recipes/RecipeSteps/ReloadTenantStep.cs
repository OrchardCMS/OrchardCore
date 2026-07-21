using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps;

/// <summary>
/// Recipe step that requests a tenant reload on completion of the recipe. Settings written by the
/// generic "Settings" step (e.g. CORS policies, which are cached in <c>CorsOptions</c> at startup)
/// otherwise only take effect after a feature change or a manual restart — this step lets a recipe
/// activate them immediately. Place it after the steps whose settings need to become live.
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
