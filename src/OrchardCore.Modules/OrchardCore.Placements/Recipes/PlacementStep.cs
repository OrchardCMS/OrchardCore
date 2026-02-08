using System.Text.Json.Nodes;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Placements.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Placements.Recipes;

/// <summary>
/// This recipe step creates a set of placements.
/// </summary>
[Obsolete("Implement IRecipeDeploymentStep instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed class PlacementStep : NamedRecipeStepHandler
#pragma warning restore CS0618
{
    private readonly PlacementsManager _placementsManager;

    public PlacementStep(PlacementsManager placementsManager)
        : base("Placements")
    {
        _placementsManager = placementsManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step.TryGetPropertyValue("Placements", out var jsonNode) && jsonNode is JsonObject templates)
        {
            foreach (var property in templates)
            {
                var name = property.Key;
                var value = property.Value.ToObject<PlacementNode[]>();

                await _placementsManager.UpdateShapePlacementsAsync(name, value);
            }
        }
    }
}
