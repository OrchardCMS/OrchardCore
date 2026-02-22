using System.Text.Json.Nodes;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Recipes;

/// <summary>
/// This recipe step creates a set of feature profiles.
/// </summary>
[Obsolete("Implement IRecipeDeploymentStep instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed class FeatureProfilesStep : NamedRecipeStepHandler
#pragma warning restore CS0618
{
    private readonly FeatureProfilesManager _featureProfilesManager;

    public FeatureProfilesStep(FeatureProfilesManager featureProfilesManager)
        : base("FeatureProfiles")
    {
        _featureProfilesManager = featureProfilesManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
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
