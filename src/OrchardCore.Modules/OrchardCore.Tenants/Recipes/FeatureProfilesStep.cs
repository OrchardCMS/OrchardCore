using System.Text.Json.Nodes;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Recipes;

/// <summary>
/// This recipe step creates a set of feature profiles.
/// </summary>
public sealed class FeatureProfilesStep : IRecipeStepHandler
{
    private readonly FeatureProfilesManager _featureProfilesManager;

    public FeatureProfilesStep(FeatureProfilesManager featureProfilesManager)
    {
        _featureProfilesManager = featureProfilesManager;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "FeatureProfiles", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (context.Step.TryGetPropertyValue("FeatureProfiles", out var jsonNode) && jsonNode is JsonObject featureProfiles)
        {
            foreach (var property in featureProfiles)
            {
                var name = property.Key;
                var value = property.Value.ToObject<FeatureProfile>();

                await _featureProfilesManager.UpdateFeatureProfileAsync(name, value);
            }
        }
    }
}
