using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Placements.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Placements.Recipes
{
    /// <summary>
    /// This recipe step creates a set of placements.
    /// </summary>
    public class PlacementStep : IRecipeStepHandler
    {
        private readonly PlacementsManager _placementsManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public PlacementStep(
            PlacementsManager placementsManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _placementsManager = placementsManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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
                    var value = property.Value.ToObject<PlacementNode[]>(_jsonSerializerOptions);

                    await _placementsManager.UpdateShapePlacementsAsync(name, value);
                }
            }
        }
    }
}
