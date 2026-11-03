using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Features.Recipes.Executors;

/// <summary>
/// Checks the features and themes that a <see cref="FeatureStep"/> would enable or disable, which the step silently
/// ignores when they don't exist.
/// </summary>
public sealed class FeatureStepValidator : IRecipeStepValidator
{
    private readonly IShellFeaturesManager _shellFeaturesManager;

    internal readonly IStringLocalizer S;

    public FeatureStepValidator(IShellFeaturesManager shellFeaturesManager, IStringLocalizer<FeatureStepValidator> stringLocalizer)
    {
        _shellFeaturesManager = shellFeaturesManager;
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public string StepName => "Feature";

    /// <inheritdoc />
    public async Task ValidateAsync(RecipeValidationContext context)
    {
        var step = context.Step.ToObject<FeatureStep.FeatureStepModel>();

        var available = (await _shellFeaturesManager.GetAvailableFeaturesAsync()).Select(feature => feature.Id).ToHashSet(StringComparer.Ordinal);
        var enabled = (await _shellFeaturesManager.GetEnabledFeaturesAsync()).Select(feature => feature.Id).ToHashSet(StringComparer.Ordinal);

        foreach (var id in step.Disable ?? [])
        {
            if (RecipeValidationContext.IsScript(id))
            {
                context.FeaturesMayChange = true;
                context.AddWarning("feature-not-evaluated", S["The feature '{0}' is a script, which only the execution evaluates.", id]);
            }
            else if (!available.Contains(id))
            {
                context.AddWarning("feature-not-found", S["The feature '{0}' doesn't exist or isn't available on this tenant. The step would ignore it.", id]);
            }

            context.PendingFeatures.Remove(id);
        }

        foreach (var id in step.Enable ?? [])
        {
            if (RecipeValidationContext.IsScript(id))
            {
                context.FeaturesMayChange = true;
                context.AddWarning("feature-not-evaluated", S["The feature '{0}' is a script, which only the execution evaluates.", id]);
            }
            else if (!available.Contains(id))
            {
                context.AddError("feature-not-found", S["The feature or theme '{0}' doesn't exist or isn't available on this tenant. The step would ignore it and still succeed.", id]);
            }
            else if (!enabled.Contains(id))
            {
                context.PendingFeatures.Add(id);
            }
        }
    }
}
