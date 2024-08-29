using System.Text.Json.Nodes;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Placements.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Placements.Recipes;

/// <summary>
/// This recipe step creates a set of placements.
/// </summary>
public sealed class PlacementStep : IRecipeStepHandler
{
    private readonly PlacementsManager _placementsManager;

    public PlacementStep(PlacementsManager placementsManager)
    {
        _placementsManager = placementsManager;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "Placements", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

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
