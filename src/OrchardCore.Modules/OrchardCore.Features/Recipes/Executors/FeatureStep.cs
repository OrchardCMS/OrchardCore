using System.Text.Json.Nodes;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Features.Recipes.Executors;

/// <summary>
/// This recipe step enables or disables a set of features.
/// </summary>
public sealed class FeatureStep : NamedRecipeStepHandler
{
    private readonly IShellFeaturesManager _shellFeaturesManager;

    public FeatureStep(IShellFeaturesManager shellFeaturesManager)
        : base("Feature")
    {
        _shellFeaturesManager = shellFeaturesManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var step = context.Step.ToObject<FeatureStepModel>();

        var features = await _shellFeaturesManager.GetAvailableFeaturesAsync();

        var featuresToDisable = features.Where(x => step.Disable?.Contains(x.Id) == true).ToArray();
        var featuresToEnable = features.Where(x => step.Enable?.Contains(x.Id) == true).ToArray();

        if (featuresToDisable.Length > 0 || featuresToEnable.Length > 0)
        {
            await _shellFeaturesManager.UpdateFeaturesAsync(featuresToDisable, featuresToEnable, true);
        }
    }

    private sealed class FeatureStepModel
    {
        public string Name { get; set; }
        public string[] Disable { get; set; }
        public string[] Enable { get; set; }
    }
}
